namespace BattleshipsBoard
{
    public class Game
    {
        public (string playerOne, string playerTwo) players = (null, null);


        public Game(string player) {
            players.playerOne = player;
        }
    }
}