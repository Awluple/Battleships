using System;
using System.Collections.Generic;

using System.Diagnostics;

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
        Cruiser = 3,
        Destroyer = 2,
        Submarine = 1,
    }

    public struct Range
{
    public Range(int x, int size)
    {
        min = x - size;
        max = x + size;
    }

    public int min { get; }
    public int max { get; }

    public override string ToString() => $"({min}, {max})";
}

    public class GameBoard
    {
        private ShipStatus[,] board = new ShipStatus[12,12];

        public Dictionary<ShipsClasses,int> shipsLeft {get; set;}

        public GameBoard() {
            this.shipsLeft = new Dictionary<ShipsClasses, int> {
            {ShipsClasses.Carrier, 1},
            {ShipsClasses.Battleship, 1},
            {ShipsClasses.Cruiser, 1},
            {ShipsClasses.Destroyer, 2},
            {ShipsClasses.Submarine, 2}
        };
            this.board[5,5] = ShipStatus.Healthy;
        }

        private int GetSize((int row, int column) startPoint, (int row, int column) endPoint) {
            if(startPoint.row != endPoint.row) {
                return (startPoint.column > endPoint.column ? startPoint.column - endPoint.column : endPoint.column - startPoint.column) + 1;
            } else {
                return (startPoint.row > endPoint.row ? startPoint.row - endPoint.row : endPoint.row - startPoint.row) + 1;
            }
        }

        public bool CheckPlacement(ShipsClasses ship, int column, int row) {
            // Debug.WriteLine($"{row} - {column}");
            if(column == 0 || column == 12 || row == 0 || row == 12){
                 return false;
            };

            if(column + (int)ship >= 12){
                return false;
            }


        return true;
        }

        public bool IsAvialiavle(int row, int column) {
            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                Debug.Write($"Row: {row} - Index: {rowIndex}\n");
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex > 11 || rowIndex < 0 || rowIndex > 11) return true;
                    if(board[rowIndex, columnIndex] != ShipStatus.Empty){
                        Debug.WriteLine($"{rowIndex} - {columnIndex}");
                        return false;
                    };
                }
            }
            return true;
        }

        public bool PlaceShip((int row, int column) startPoint, (int row, int column) endPoint) {
            int size = GetSize(startPoint, endPoint);

            ShipsClasses shipType = (ShipsClasses)size;
            
            // board[startPoint.row, startPoint.column] = ShipStatus.Healthy;


            return true;
        }

    }
}