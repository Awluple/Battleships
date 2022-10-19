using System.Collections.Generic;

namespace Battleships.Board
{

/// <summary>Struct used to hold coordinates on the game board</summary>
/// <param name="col">Column on the game board</param>
/// <param name="row">Row on the game board</param>
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