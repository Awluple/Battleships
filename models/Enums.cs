namespace Battleships.Board
{
    /// <summary>Representation of a ship health status</summary>
    public enum ShipStatus
    {
        Empty = 0,
        Healthy,
        Damaged,
        Destroyed,
    }
    /// <summary>Classes of the ships, number represents lenght of a ship</summary>
    public enum ShipsClasses
    {
        Carrier = 5,
        Battleship = 4,
        Cruiser = 3,
        Destroyer = 2,
        Submarine = 1,
    }
    /// <summary>Ship oriantation on the board</summary>
    public enum ShipOrientation {
        Horizontal,
        Vertical
    }
}