using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Newtonsoft.Json.Linq;

using BattleshipsShared.Communication;
using BattleshipsShared.Models;

namespace Battleships.Board
{
    /// <summary>Main game board, handles inputs from the user and updating game board on the UI</summary>
    /// <param name="game">Currenty connected game</param>
    /// <param name="gameBoard">User's board with placed ships</param>
    /// <param name="isStartingPlayer">True if the users has the first move</param>
    public partial class MainBoard : BoardPage
    {
        private bool playerTurn = true;
        private bool lockShooting = false;
        private Border[,] playerBorders;
        private Border[,] opponentBorders;
        private PlayerBoard opponentBoard = new PlayerBoard();
        
    
        public MainBoard(Game game, PlayerBoard gameBoard, bool isStartingPlayer) : base(game) {
            InitializeComponent();
            Game.WebSocketMessage += this.EnemyDisconnected;

            this.board = gameBoard;
            this.opponentBorders = this.CreateGrid(opponentGrid, Cursors.Hand);
            this.playerBorders = this.CreateGrid(playerGrid, Cursors.Arrow);

            this.AddEvents();
            setTurnInfo(isStartingPlayer);
            paintPlayerBoard();

            this.DataContext = board;
            this.playerTurn = isStartingPlayer;
            Application.Current.MainWindow.Height = 970;
            Game.WebSocketMessage += this.GetShotResult;
            this.Overlay_Disconnected = Disconnected_Overlay;

            if(!isStartingPlayer) {
                lockShooting = true;
            }
        }
        /// <summary>Displays error overlay if the enemy has disconnected</summary>
        private void EnemyLeft(object sender, WebSocketContextEventArgs e) {
            if(e.message.requestType != RequestType.OpponentLeft && e.message.requestType != RequestType.OpponentConnectionLost) return;
            Game.WebSocketMessage -= this.EnemyLeft;
            
            Overlay_rematchPropositon.Text = "Your opponent has left the game";
            Overlay_rematchPropositon.Visibility = Visibility.Visible;
            Overlay_rematchButton.IsEnabled = false;
        }

        private void setTurnInfo(bool isPlayerTurn) {
            turnInfo.Text = isPlayerTurn ? "Your turn" : "Opponent's turn";
        }

        /// <summary>Copies the board from thr gameBoard object to the UI</summary>
        private void paintPlayerBoard() {
            int[,] serialized = board.SerializeBoard();

            for (var col = 0; col < 10; col++) {
                for (var row = 0; row < 10; row++) {
                    if(serialized[row, col] == 1) {
                        ChangeCellColor(this.board, playerBorders, Brushes.GreenYellow, col + 1, row + 1, true);
                    };
                }
            }
        }
        /// <summary>Adds mouse events to every cell at the game board</summary>
        private void AddEvents() {
            foreach (Border item in opponentGrid.Children)
            {
                item.MouseEnter += over;
                item.MouseLeave += leaves;
                item.MouseLeftButtonDown += Shot;
            }
        }

        private void over(object sender, MouseEventArgs e) {
            e.Handled = true;
            if(!playerTurn) {
                return;
            }
            Border br = (Border)e.Source;
            if(br.Background == Brushes.LightBlue) {
                ChangeCellColor(this.opponentBoard, this.opponentBorders, Brushes.DarkRed, Grid.GetColumn(br), Grid.GetRow(br));
            }
        }

        private void leaves(object sender, MouseEventArgs e) {
            if(e.Source is Border) {
                Border br = (Border)e.Source;
                if(br.Background == Brushes.DarkRed) {
                    ChangeCellColor(this.opponentBoard, this.opponentBorders, Brushes.LightBlue, Grid.GetColumn(br), Grid.GetRow(br));
                }
            }
        }
        /// <summary>Process shot result information recieved from the server</summary>
        private void GetShotResult(object sender, WebSocketContextEventArgs e) {
            if(!(e.message.requestType == RequestType.ShotResult || e.message.requestType == RequestType.GameResult)) return;
            Dictionary<string, JObject> data = Message.DeserializeData(e.message);

            PlayerBoard activeBoard = !playerTurn ? this.board : this.opponentBoard;
            Border[,] borders = !playerTurn ? this.playerBorders : this.opponentBorders;

            if(e.message.requestType == RequestType.GameResult) { // finish the game if request type is final game result
                GameResult gameResult = data["gameResult"].ToObject<GameResult>();
                this.MarkAsDestroyed(gameResult.column, gameResult.row, activeBoard, borders);
                this.FinishGame(gameResult);
                return;
            }

            ShotResult result = data["shotResult"].ToObject<ShotResult>();

            if(result.shotStatus == ShotStatus.Miss){
                hitInfo.Text = "Miss!";
                setTurnInfo(!playerTurn);
                ChangeCellColor(activeBoard, borders, Brushes.Gray, result.column + 1, result.row + 1, true);
                if(playerTurn) { // lock or unluck inputs
                    lockShooting = true;
                    playerTurn = false;
                } else {
                    lockShooting = false;
                    playerTurn = true;
                }

            } else {
                if(result.shotStatus == ShotStatus.Hit) {
                    hitInfo.Text = "Hit!";
                    activeBoard.Update(result.column, result.row, ShotStatus.Hit);
                    ChangeCellColor(activeBoard, borders, Brushes.Orange, result.column + 1, result.row + 1, true);
                } else if(result.shotStatus == ShotStatus.Destroyed) {
                    this.MarkAsDestroyed(result.column, result.row, activeBoard, borders);
                }
                if(playerTurn) {
                    lockShooting = false;
                }
            }

        }
        /// <summary>Marks a ship as destroyed by painting it red</summary>
        /// <param name="column">Column number with a ship segment</param>
        /// <param name="row">Row number with a ship segment</param>
        /// <param name="activeBoard">PlayerBoard object to update</param>
        /// <param name="borders">Game board borders from the UI</param>
        private void MarkAsDestroyed(int column, int row, PlayerBoard activeBoard, Border[,] borders) {
            hitInfo.Text = "Destroyed!";
                    
            List<Coords> destroyedShipCoords = activeBoard.GetShipCoords(column, row);
            foreach (Coords coord in destroyedShipCoords)
            {
                activeBoard.Update(coord.Column, coord.Row, ShotStatus.Destroyed);
                ChangeCellColor(activeBoard, borders, Brushes.OrangeRed, coord.Column + 1, coord.Row + 1, true);
            }
        }
        /// <summary>Shows the finished game overlay and the game result</summary>
        /// <param name="gameResult">Final game result</param>
        private void FinishGame(GameResult gameResult) {
            Game.WebSocketMessage += this.EnemyLeft;
            Game.WebSocketMessage -= this.EnemyDisconnected;
            string winner = gameResult.winner == Settings.userId ? "You won!" : "You lost.";
            Game.WebSocketMessage += this.Rematch;

            Overlay.Visibility = Visibility.Visible;
            Overlay_winner.Text = winner;
        }

        private void Disconnect(object sender, RoutedEventArgs e) {
            Game.CloseConnection();
            Application.Current.MainWindow.Height = 970;
            Uri uri = new Uri("../views/menu/MainMenu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(uri);
        }
        /// <summary>Sends a proposal for a rematch</summary>
        private void RematchProposition(object sender, RoutedEventArgs e) {
            Overlay_rematchPropositon.Text = "Rematch proposition sent!";
            Overlay_rematchPropositon.Visibility = Visibility.Visible;
            Overlay_rematchButton.IsEnabled = false;
            game.Rematch();
        }
        /// <summary>Handles a rematch proposal. Displays information on the UI if the opponent wants a rematch or redirects to the Ship Placement page if both players want a rematch</summary>
        private void Rematch(object sender, WebSocketContextEventArgs e) {
            if(e.message.requestType != RequestType.RematchAccepted && e.message.requestType != RequestType.RematchProposition) return;
            if(e.message.requestType == RequestType.RematchProposition) {
                Overlay_rematchPropositon.Text = "Your opponent wants a rematch!";
                Overlay_rematchPropositon.Visibility = Visibility.Visible;
            } else {
                Game.WebSocketMessage -= this.Rematch;
                MainWindow parentWindow = Window.GetWindow(this) as MainWindow;
                var page = new ShipsPlacement(this.game);
                (Application.Current.MainWindow as MainWindow).MainContainer.NavigationService.Navigate(page);
            }
        }
        /// <summary>Valides if the targeted cell is valid and sends shot data to the server</summary>
        private void Shot(object sender, MouseEventArgs e) {
            e.Handled = true;
            if(!playerTurn || lockShooting) {
                return;
            }
            if(e.Source is Border) {
                Border br = (Border)e.Source;
                if(br.Background == Brushes.Gray || br.Background == Brushes.Orange || br.Background == Brushes.OrangeRed) { // prevent multiple shots at the same cell
                    return;
                }
                if(Grid.GetRow(br) == 0 || Grid.GetColumn(br) == 0) { // ignore label cells
                    return;
                }
                lockShooting = true;
                this.game.Shot(Grid.GetRow(br) - 1, Grid.GetColumn(br) - 1);
            }
        }
    }
}