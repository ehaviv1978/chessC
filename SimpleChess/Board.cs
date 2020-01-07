using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChess
{
    class Board
    {
        public Checker[] board1d = new Checker[64];
        public Checker[,] board2d = new Checker[8, 8];
    }
}
