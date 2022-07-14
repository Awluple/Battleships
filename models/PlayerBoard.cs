using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using BattleshipsShared.Communication;

namespace Battleships.Board
{
    public class PlayerBoard : INotifyPropertyChanged
    {

        private Dictionary<ShipsClasses,int> _shipsLeft {get; set;}

        protected ShipStatus[,] board = new ShipStatus[12,12];

        public Dictionary<ShipsClasses,int> shipsLeft
        {
            get { return _shipsLeft; }
            set
            {
                _shipsLeft = value;
                OnPropertyChanged();
            }
        }

        public PlayerBoard() {
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

        public int[,] SerializeBoard() {
            int[,] serialized = new int[10,10];

            for (var col = 0; col < 10; col++) {
                for (var row = 0; row < 10; row++) {
                    serialized[row, col] = board[row, col] == ShipStatus.Empty ? 0 : 1;
                }
            }

            return serialized;
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

        public void Update(int column, int row, ShotStatus status){
            if(status == ShotStatus.Hit) {
                this.board[row, column] = ShipStatus.Damaged;
            } else if(status == ShotStatus.Destroyed) {
                this.board[row, column] = ShipStatus.Destroyed;
            }
        }

        public bool IsAvialiavle(ShipsClasses ship, ShipOrientation orientation, int column, int row) {
            //check first 3x3 area
            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex >= 11 || rowIndex < 0 || rowIndex >= 11) continue;
                    if(board[rowIndex, columnIndex] != ShipStatus.Empty){
                        return false;
                    };
                }
            }
            //check the text 1x3 area for every ship segment
            for(int i = 1; i <= (int)ship; i++) {
                int newCol = orientation == ShipOrientation.Vertical ? column + i : column;
                int newRow = orientation == ShipOrientation.Horizontal ? row + i : row;
                for (var j = -1; j <= 1; j++) {

                    int columnIndex = orientation == ShipOrientation.Horizontal ? newCol + j : newCol;
                    int rowIndex = orientation == ShipOrientation.Vertical ? newRow + j : newRow;

                    if(columnIndex < 0 || columnIndex > 11 || rowIndex < 0 || rowIndex > 11) continue;

                    if(board[rowIndex, columnIndex] != ShipStatus.Empty){
                        return false;
                    };
                }
            }
            
            return true;
        }

        public ShipStatus getOccupation(int column, int row) {
            return this.board[row, column];
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
                board[ro, col] = ShipStatus.Healthy;
            }

            var left = this.shipsLeft;
            left[ship] = left[ship] - 1;

            this.shipsLeft = left; 

            return true;
        }

        public List<Coords> GetShipCoords(int column, int row) {
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

                            int vector;

                            if(orientation == ShipOrientation.Horizontal) {
                                vector = rowIndex - row;
                            } else {
                                vector = columnIndex - column;
                            }

                            if(firstSide.tried == false) {
                                firstSide.tried = true;
                                firstSide.toMarkAsDestroyed = GetNextSegment(orientation, vector, column, row);
                            } else {
                                secondSide.tried = true;
                                secondSide.toMarkAsDestroyed = GetNextSegment(orientation, vector, column, row);
                            }
                        };
                    }
                }

                firstSide.toMarkAsDestroyed.AddRange(secondSide.toMarkAsDestroyed);
                Coords initialCoords = new Coords(column, row);
                firstSide.toMarkAsDestroyed.Add(initialCoords);
                return firstSide.toMarkAsDestroyed;

            }

            private List<Coords> GetNextSegment(ShipOrientation orientation, int vector, int column, int row, int segment = 1, List<Coords> toMarkAsDestroyed = null) {
                int newCol = orientation == ShipOrientation.Vertical ? column + segment * vector : column;
                int newRow = orientation == ShipOrientation.Horizontal ? row + segment * vector : row;

                if(toMarkAsDestroyed == null) {
                    toMarkAsDestroyed = new List<Coords>();
                }

                if(newCol < 0 || newCol > 11 || newRow < 0 || newRow > 11) return toMarkAsDestroyed;

                if (board[newRow, newCol] == ShipStatus.Empty) {
                    return toMarkAsDestroyed;
                } else {
                    Coords cor = new Coords(newCol, newRow);
                    toMarkAsDestroyed.Add(cor);
                    return GetNextSegment(orientation, vector, column, row, segment + 1, toMarkAsDestroyed);
                };
            }
        }
}