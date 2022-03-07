using System;
using System.Collections.Generic;

namespace Battleships.Board
{
    public enum ShipStatus
    {
        Empty = 0,
        Healthy,
        Damaged,
        Destroyed,
    }

    public enum ShipsClasses
    {
        Carrier = 5,
        Battleship = 4,
        Cruser = 3,
        Destroyer = 2,
        Submarine = 1,
    }
    public class GameBoard
    {
        private ShipStatus[,] board = new ShipStatus[12,12];

        Dictionary<ShipsClasses,int> shipsLeft = new Dictionary<ShipsClasses, int> {
            {ShipsClasses.Carrier, 1},
            {ShipsClasses.Battleship, 1},
            {ShipsClasses.Cruser, 1},
            {ShipsClasses.Destroyer, 2},
            {ShipsClasses.Submarine, 2}
        };

        public GameBoard() {
            
        }

        private int GetSize((int row, int column) startPoint, (int row, int column) endPoint) {
            if(startPoint.row != endPoint.row) {
                return (startPoint.column > endPoint.column ? startPoint.column - endPoint.column : endPoint.column - startPoint.column) + 1;
            } else {
                return (startPoint.row > endPoint.row ? startPoint.row - endPoint.row : endPoint.row - startPoint.row) + 1;
            }
        }

        public bool IsAvialiavle(int row, int column) {
            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(board[rowIndex, columnIndex] != ShipStatus.Empty) return false;
                }
            }
            return true;
        }

        public bool PlaceShip((int row, int column) startPoint, (int row, int column) endPoint) {
            int size = GetSize(startPoint, endPoint);

            ShipsClasses shipType = (ShipsClasses)size;
            
            board[startPoint.row, startPoint.column] = ShipStatus.Healthy;


            return true;
        }

    }
}