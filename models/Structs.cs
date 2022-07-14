using System.Collections.Generic;

namespace Battleships.Board
{

public struct Coords
    {
        public Coords(int col, int row)
        {
            Column = col;
            Row = row;
        }
    
        public int Column { get; }
        public int Row { get; }
    }

}