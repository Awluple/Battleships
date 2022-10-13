using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using BattleshipsShared.Communication;

namespace Battleships.Board
{
    /// <summary>Class that holds and manages all the information about a user's game board</summary>
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

        /// <summary>Converts a ShipStatus array into an int array</summary>
        /// <returns>A 2d int array representation of a player's board</returns>
        public int[,] SerializeBoard() {
            int[,] serialized = new int[10,10];

            for (var col = 0; col < 10; col++) {
                for (var row = 0; row < 10; row++) {
                    serialized[row, col] = board[row, col] == ShipStatus.Empty ? 0 : 1;
                }
            }

            return serialized;
        }
        /// <summary>Checks if it is possible to place a ship in a selected location</summary>
        /// <returns>True if it is possible to place a ship, otherwise false</returns>
        /// <param name="ship">Type of ship to be placed</param>
        /// <param name="orientation">Orientation in which a ship will be placed</param>
        /// <param name="column">Column number at the first segment of a ship</param>
        /// <param name="row">Row number at the first segment of a ship</param>
        public bool CheckPlacement(ShipsClasses ship, ShipOrientation orientation, int column, int row) {
            if(column < 0 || row < 0){
                 return false;
            };

            // check if all of the ships segments are in the range of the game board
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

        /// <summary>Updates the game board at selected location</summary>
        /// <param name="column">Column number</param>
        /// <param name="row">Row number</param>
        /// <param name="status">Status of a given shot</param>
        public void Update(int column, int row, ShotStatus status){
            if(status == ShotStatus.Hit) {
                this.board[row, column] = ShipStatus.Damaged;
            } else if(status == ShotStatus.Destroyed) {
                this.board[row, column] = ShipStatus.Destroyed;
            }
        }
        
        /// <summary>Checks an area where a ship is to be placed. It checks if there is no other ships in a given location or around the ship to be placed.
        /// It first checks 3x3 area around the first segment of the ship, then it checks 1x3 area for every next ship segment</summary>
        /// <param name="ship">ShipsClasses of a ship to be placed</param>
        /// <param name="orientation">ShipOrientation of a ship to check</param>
        /// <param name="column">Firts segment column number</param>
        /// <param name="row">Firts segment row number</param>
        /// <returns>True if a location if free, otherwise false</returns>
        private bool IsAvialiavle(ShipsClasses ship, ShipOrientation orientation, int column, int row) {
            //check the first 3x3 area
            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex >= 11 || rowIndex < 0 || rowIndex >= 11) continue; // do not go outside the board range
                    if(board[rowIndex, columnIndex] != ShipStatus.Empty){ // Break if a location is not empty
                        return false;
                    };
                }
            }
            //check the next 1x3 area for every ship segment
            for(int i = 1; i <= (int)ship; i++) { // ship segment
                int newCol = orientation == ShipOrientation.Vertical ? column + i : column;
                int newRow = orientation == ShipOrientation.Horizontal ? row + i : row;
                for (var j = -1; j <= 1; j++) { // 1x3 area

                    int columnIndex = orientation == ShipOrientation.Horizontal ? newCol + j : newCol;
                    int rowIndex = orientation == ShipOrientation.Vertical ? newRow + j : newRow;

                    if(columnIndex < 0 || columnIndex > 11 || rowIndex < 0 || rowIndex > 11) continue; // do not go outside the board range

                    if(board[rowIndex, columnIndex] != ShipStatus.Empty){ // Break if a location is not empty
                        return false;
                    };
                }
            }
            
            return true;
        }
        
        /// <summary>Returns ShipStatus in the board at a given coors</summary>
        /// <param name="column">Column number</param>
        /// <param name="row">Row number</param>
        /// <returns>ShipStatus at the given coords</returns>
        public ShipStatus getOccupation(int column, int row) {
            return this.board[row, column];
        }

        /// <summary>Places a ship on the board. It also checks if the given location is not already occupied</summary>
        /// <param name="ship">ShipsClasses of a ship to be placed</param>
        /// <param name="orientation">ShipOrientation of a ship to be placed</param>
        /// <param name="column">Firts segment column number</param>
        /// <param name="row">Firts segment row number</param>
        /// <returns>True if a ship has been successfully placed, otherwise false</returns>
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
            this.shipsLeft = left; // remove the ship from the list of ships left to be placed
            return true;
        }
        
        /// <summary>Used in marking a ship is destroyed. Starting at a given location it searches for all nearby fields on the board to collect Coords of all segments of a ship</summary>
        /// <param name="column">Column number</param>
        /// <param name="row">Row number</param>
        /// <returns>A List of Coords with all segments of the ship </returns>
        public List<Coords> GetShipCoords(int column, int row) {
                (bool tried, List<Coords> toMarkAsDestroyed) firstSide = (false, new List<Coords>());
                (bool tried, List<Coords> toMarkAsDestroyed) secondSide = (false, new List<Coords>());

                for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                    for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                        if(columnIndex < 0 || columnIndex >= 11 || rowIndex < 0 || rowIndex >= 11) continue; // do not go outside the board range
                        if(board[rowIndex, columnIndex] != ShipStatus.Empty){
                            ShipOrientation orientation = rowIndex - row != 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical; // determine in which direction the ship is turned

                            if(rowIndex == row && columnIndex == column) { // ignore initial location
                                continue;
                            }

                            int direction; // determines a side of the ship to check
                            if(orientation == ShipOrientation.Horizontal) {
                                direction = rowIndex - row;
                            } else {
                                direction = columnIndex - column;
                            }

                            if(firstSide.tried == false) {
                                firstSide.tried = true;
                                firstSide.toMarkAsDestroyed = GetNextSegment(orientation, direction, column, row);
                            } else {
                                secondSide.tried = true;
                                secondSide.toMarkAsDestroyed = GetNextSegment(orientation, direction, column, row);
                            }
                        };
                    }
                }

                // joins all coords into a one list
                firstSide.toMarkAsDestroyed.AddRange(secondSide.toMarkAsDestroyed);
                Coords initialCoords = new Coords(column, row);
                firstSide.toMarkAsDestroyed.Add(initialCoords);

                return firstSide.toMarkAsDestroyed;

            }

            /// <summary>Adds next segment to the list of the ship's segments coords</summary>
            /// <param name="orientation">The ShipOrientation of the shp</param>
            /// <param name="direction">In which direction the function should move</param>
            /// <param name="column">Column number</param>
            /// <param name="row">Row number</param>
            /// <param name="segment">Currently checked segment of the ship</param>
            /// <param name="toMarkAsDestroyed">List of Coords of ship segments from the previous method executions</param>
            private List<Coords> GetNextSegment(ShipOrientation orientation, int direction, int column, int row, int segment = 1, List<Coords> toMarkAsDestroyed = null) {
                // calculate the coords to be checked
                int newCol = orientation == ShipOrientation.Vertical ? column + segment * direction : column;
                int newRow = orientation == ShipOrientation.Horizontal ? row + segment * direction : row;

                if(toMarkAsDestroyed == null) {
                    toMarkAsDestroyed = new List<Coords>();
                }

                if(newCol < 0 || newCol > 11 || newRow < 0 || newRow > 11) return toMarkAsDestroyed; // finish if the next segment should be outside the board

                if (board[newRow, newCol] == ShipStatus.Empty) {
                    return toMarkAsDestroyed; // finish if the next board field is empty
                } else {
                    Coords coor = new Coords(newCol, newRow);
                    toMarkAsDestroyed.Add(coor);
                    return GetNextSegment(orientation, direction, column, row, segment + 1, toMarkAsDestroyed); // check the next board field
                };
            }
        }
}