using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;

namespace MinecraftTK {
    class Game : GameWindow {
        private const int SCREEN_WIDTH = 1280;
        private const int SCREEN_HEIGHT = 720;

        private const float PLAYER_SPEED = 0.015f;
        private const float BALL_SPEED = 0.005f;

        private const float PLAYER_HEIGHT = 0.30f;
        private const float PLAYER_WIDTH = 0.05f;

        private const float BALL_SIZE = 0.05f;

        private static readonly Color PLAYER_COLOR = Color.White;
        private static readonly Color BALL_COLOR = Color.CornflowerBlue;

        // Direction in degrees
        private float ball_direction;
        private float player_l_direction;
        private float player_r_direction;

        private float ball_x_position;
        private float ball_y_position;

        private float player_l_y_position;
        private float player_r_y_position;

        private int player_r_score;
        private int player_l_score;

        private int vert_id_ball;
        private int vert_id_player_l;
        private int vert_id_player_r;

        public Game() : base(SCREEN_WIDTH, SCREEN_HEIGHT, GraphicsMode.Default, "PongTK") {
            
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            vert_id_ball = GL.GenBuffer();
            vert_id_player_l = GL.GenBuffer();
            vert_id_player_r = GL.GenBuffer();

            ResetBall();

            Render();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            //Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref projection);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e) {
            base.OnKeyDown(e);

            if (e.Key == Key.Down) {
                player_r_direction = -1;
            }

            else if(e.Key == Key.Up) {
                player_r_direction = +1;
            }

            else if (e.Key == Key.W) {
                player_l_direction = +1;
            }

            else if (e.Key == Key.S) {
                player_l_direction = -1;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e) {
            base.OnKeyUp(e);

            if (e.Key == Key.Down && player_r_direction == -1) {
                player_r_direction = 0;
            }

            else if (e.Key == Key.Up && player_r_direction == 1) {
                player_r_direction = 0;
            }

            if (e.Key == Key.W && player_l_direction == 1) {
                player_l_direction = 0;
            }

            else if (e.Key == Key.S && player_l_direction == -1) {
                player_l_direction = 0;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            UpdatePositions();

            Render();
        }

        private void Render() {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ClearColor(Color.Black);

            GL.EnableClientState(ArrayCap.VertexArray);

            UpdatePositions();

            RenderPlayers();

            CheckBallPosition();

            RenderBall();

            GL.Flush();

            SwapBuffers();
        }

        private void UpdatePositions() {
            player_l_y_position += player_l_direction * PLAYER_SPEED;
            player_r_y_position  += player_r_direction * PLAYER_SPEED;

            // Check borders
            if (player_l_y_position < -1 + (PLAYER_HEIGHT / 2)) {
                player_l_y_position = -1 + (PLAYER_HEIGHT / 2);
            }

            if (player_l_y_position > 1 - (PLAYER_HEIGHT / 2)) {
                player_l_y_position = 1 - (PLAYER_HEIGHT / 2);
            }

            if (player_r_y_position < -1 + (PLAYER_HEIGHT / 2)) {
                player_r_y_position = -1 + (PLAYER_HEIGHT / 2);
            }

            if (player_r_y_position > 1 - (PLAYER_HEIGHT / 2)) {
                player_r_y_position = 1 - (PLAYER_HEIGHT / 2);
            }

            ball_x_position += BALL_SPEED * (float)Math.Cos((-1 * (ball_direction - 90.0f)) * (Math.PI / 180));
            ball_y_position += BALL_SPEED * (float)Math.Sin((-1 * (ball_direction - 90.0f)) * (Math.PI / 180));
        }

        private void CheckBallPosition() {
            if (ball_y_position > 1 - (BALL_SIZE / 2) || 
                ball_y_position < -1 + (BALL_SIZE / 2)) {

                ball_direction = (180 - ball_direction);
            }

            // Check if bouncing with a player
            if ((ball_x_position > 1 - (BALL_SIZE / 2) - (PLAYER_WIDTH / 2) || 
                ball_x_position < -1 + (BALL_SIZE / 2) + (PLAYER_WIDTH / 2)) &&
                ((Math.Abs((ball_y_position + 1) - (player_l_y_position + 1)) < ((PLAYER_HEIGHT + BALL_SIZE) / 2) ||
                 Math.Abs((ball_y_position + 1) - (player_r_y_position + 1)) < ((PLAYER_HEIGHT + BALL_SIZE) / 2)))) {

                ball_direction = (360 - ball_direction); 
            }

            // Check if the ball is past the player
            if (ball_x_position > 1 - (BALL_SIZE / 2)) {
                Console.WriteLine("The left player scored");
                player_l_score++;
                ResetBall();
            }

            if (ball_x_position < -1 + (BALL_SIZE / 2)) {
                Console.WriteLine("The right player scored");
                player_r_score++;
                ResetBall();
            }
        }

        private void RenderPlayers() {
            GL.Color3(PLAYER_COLOR);

            // Left player
            var player_l_vbo = new Vector2[] {
                new Vector2(-1,                      player_l_y_position + (PLAYER_HEIGHT / 2)),
                new Vector2(-1 + (PLAYER_WIDTH / 2), player_l_y_position + (PLAYER_HEIGHT / 2)),
                new Vector2(-1 + (PLAYER_WIDTH / 2), player_l_y_position - (PLAYER_HEIGHT / 2)),
                new Vector2(-1,                      player_l_y_position - (PLAYER_HEIGHT / 2))
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vert_id_player_l);

            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                       (IntPtr)(Vector2.SizeInBytes * player_l_vbo.Length),
                       player_l_vbo,
                       BufferUsageHint.StaticDraw);

            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, 0);

            GL.DrawArrays(PrimitiveType.Quads, 0, player_l_vbo.Length);

            // Right player
            var player_r_vbo = new Vector2[] {
                new Vector2(1 - (PLAYER_WIDTH / 2), player_r_y_position + (PLAYER_HEIGHT / 2)),
                new Vector2(1,                      player_r_y_position + (PLAYER_HEIGHT / 2)),
                new Vector2(1,                      player_r_y_position - (PLAYER_HEIGHT / 2)),
                new Vector2(1 - (PLAYER_WIDTH / 2), player_r_y_position - (PLAYER_HEIGHT / 2))
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vert_id_player_r);

            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                       (IntPtr)(Vector2.SizeInBytes * player_r_vbo.Length),
                       player_r_vbo,
                       BufferUsageHint.StaticDraw);

            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, 0);

            GL.DrawArrays(PrimitiveType.Quads, 0, player_r_vbo.Length);
        }

        private void RenderBall() {
            GL.Color3(BALL_COLOR);

            var ball_vbo = new Vector2[] {
                new Vector2(ball_x_position - (BALL_SIZE / 2), ball_y_position + (BALL_SIZE / 2)),
                new Vector2(ball_x_position + (BALL_SIZE / 2), ball_y_position + (BALL_SIZE / 2)),
                new Vector2(ball_x_position + (BALL_SIZE / 2), ball_y_position - (BALL_SIZE / 2)),
                new Vector2(ball_x_position - (BALL_SIZE / 2), ball_y_position - (BALL_SIZE / 2))
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vert_id_ball);

            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                                   (IntPtr)(Vector2.SizeInBytes * ball_vbo.Length),
                                   ball_vbo,
                                   BufferUsageHint.StaticDraw);

            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, 0);

            GL.DrawArrays(PrimitiveType.Quads, 0, ball_vbo.Length);
        }

        private void ResetBall() {
            var r = new Random();

            ball_direction = r.Next(20, 70);

            ball_x_position = 0.0f;
            ball_y_position = 0.0f;
        }
    }
}
