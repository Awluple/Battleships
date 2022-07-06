using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using System.Diagnostics;

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

    public class OpponentBoard
    {
        private ShipStatus[,] board = new ShipStatus[12,12];

        public OpponentBoard() {
        }

        public void Update(int col, int row){
            
        }

        public List<Coords> CheckShipHealth(int column, int row) {
            (bool tried, List<Coords> toMarkAsDestroyed) firstSide = (false, new List<Coords>());
            (bool tried, List<Coords> toMarkAsDestroyed) secondSide = (false, new List<Coords>());

            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex >= 11 || rowIndex < 0 || rowIndex >= 11) continue;
                    if(board[rowIndex, columnIndex] != ShipStatus.Empty){
                        ShipOrientation orientation = rowIndex - row != 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;

                        if(rowIndex == row && columnIndex == column) {
                            continue;
                        }
                        
                        if(firstSide.tried == false) {
                            firstSide.tried = true;
                            firstSide.toMarkAsDestroyed = CheckNextSegment(orientation, 1, column, row);
                        } else {
                            secondSide.tried = true;
                            secondSide.toMarkAsDestroyed = CheckNextSegment(orientation, -1, column, row);
                        }
                    };
                }
            }

            if(firstSide.toMarkAsDestroyed.Count == 0 && secondSide.toMarkAsDestroyed.Count == 0) {
                return new List<Coords>();
            } else {
                firstSide.toMarkAsDestroyed.AddRange(secondSide.toMarkAsDestroyed);
                Coords first = new Coords(column, row);
                firstSide.toMarkAsDestroyed.Add(first);
                return firstSide.toMarkAsDestroyed;
            }

        }

        private List<Coords> CheckNextSegment(ShipOrientation orientation, int vector, int column, int row, int segment = 1, List<Coords> toMarkAsDestroyed = null) {
            int newCol = orientation == ShipOrientation.Vertical ? column + segment * vector : column;
            int newRow = orientation == ShipOrientation.Horizontal ? row + segment * vector : row;

            if(toMarkAsDestroyed == null) {
                toMarkAsDestroyed = new List<Coords>();
            }

            if(newCol < 0 || newCol > 11 || newRow < 0 || newRow > 11) return toMarkAsDestroyed;

            if(board[newRow, newCol] == ShipStatus.Healthy){
                return new List<Coords>();
            } else if (board[newRow, newCol] == ShipStatus.Empty) {
                return toMarkAsDestroyed;
            } else {
                Coords cor = new Coords(newCol, newRow);
                toMarkAsDestroyed.Add(cor);
                return CheckNextSegment(orientation, vector, column, row, segment + 1, toMarkAsDestroyed);
            };
        }
    }
}