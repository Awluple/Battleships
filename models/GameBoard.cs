using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using System.Diagnostics;

namespace Battleships.Board
{
    public class GameBoard : INotifyPropertyChanged 
    {
        private ShipStatus[,] board = new ShipStatus[12,12];

        private Dictionary<ShipsClasses,int> _shipsLeft {get; set;}

        public Dictionary<ShipsClasses,int> shipsLeft
        {
            get { return _shipsLeft; }
            set
            {
                _shipsLeft = value;
                OnPropertyChanged();
            }
        }

        public GameBoard() {
            this.shipsLeft = new Dictionary<ShipsClasses, int> {
            {ShipsClasses.Carrier, 1},
            {ShipsClasses.Battleship, 1},
            {ShipsClasses.Cruiser, 1},
            {ShipsClasses.Destroyer, 2},
            {ShipsClasses.Submarine, 2}
        };
        }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

        public bool CheckPlacement(ShipsClasses ship, ShipOrientation orientation, int column, int row) {
            if(column < 0 || row < 0){
                 return false;
            };

            if(orientation == ShipOrientation.Vertical && column + (int)ship > 10){
                return false;
            } else if (orientation == ShipOrientation.Horizontal && row + (int)ship > 10){
                return false;
            }

            if(!IsAvialiavle(ship, orientation, column, row)) {
                return false;
            }

        return true;
        }

        public bool IsAvialiavle(ShipsClasses ship, ShipOrientation orientation, int column, int row) {
            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex > 11 || rowIndex < 0 || rowIndex > 11) continue;
                    if(board[columnIndex, rowIndex] != ShipStatus.Empty){
                        return false;
                    };
                }
            }
            for(int i = 1; i <= (int)ship; i++) {
                int newCol = orientation == ShipOrientation.Vertical ? column + i : column;
                int newRow = orientation == ShipOrientation.Horizontal ? row + i : row;
                for (var j = -1; j <= 1; j++) {

                    int columnIndex = orientation == ShipOrientation.Horizontal ? newCol + j : newCol;
                    int rowIndex = orientation == ShipOrientation.Vertical ? newRow + j : newRow;

                    if(columnIndex < 0 || columnIndex > 11 || rowIndex < 0 || rowIndex > 11) continue;

                    if(board[columnIndex, rowIndex] != ShipStatus.Empty){
                        return false;
                    };
                }
            }
            
            return true;
        }

        public bool IsOccupied(int column, int row) {
            return this.board[column, row] != ShipStatus.Empty;
        }

        public bool PlaceShip(ShipsClasses ship, ShipOrientation orientation, int column, int row) {
            if(this.shipsLeft[ship] == 0) {
                return false;
            }
            int size = (int)ship;

            if(!CheckPlacement(ship, orientation, column, row)) {
                return false;
            }

            for(int i = 0; i < size; i++) {
                int col = orientation == ShipOrientation.Vertical ? column + i : column;
                int ro = orientation == ShipOrientation.Horizontal ? row + i : row;
                board[col, ro] = ShipStatus.Healthy;
            }

            var left = this.shipsLeft;
            left[ship] = left[ship] - 1;

            this.shipsLeft = left; 

            return true;
        }

    }
}