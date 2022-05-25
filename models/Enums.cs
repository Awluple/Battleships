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

    public enum ShipOrientation {
        Horizontal,
        Vertical
    }
}