﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftTK {
    class Program {

        [STAThread]
        static void Main(string[] args) {
            using (var game = new Game()) {
                game.Run();
            }
        }
    }
}
