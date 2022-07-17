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
    public partial class MainBoard : BoardPage
    {
        private bool playerTurn = true;
        private bool lockShooting = false;
        private Border[,] playerBorders;
        private Border[,] opponentBorders;
        private PlayerBoard opponentBoard = new PlayerBoard();
        

        public MainBoard(Game game, PlayerBoard gameBoard, bool isStartingPlayer) : base(game) {
            InitializeComponent();
            this.board = gameBoard;
            this.opponentBorders = this.CreateGrid(opponentGrid);
            this.playerBorders = this.CreateGrid(playerGrid);
            this.AddEvents();
            setTurnInfo(isStartingPlayer);
            paintPlayerBoard();
            this.DataContext = board;
            this.playerTurn = isStartingPlayer;
            Application.Current.MainWindow.Height = 1200;
            Game.WebSocketMessage += this.GetShotResult;
            if(!isStartingPlayer) {
                lockShooting = true;
            }
        }

        private void setTurnInfo(bool isPlayerTurn) {
            turnInfo.Text = isPlayerTurn ? "Your turn" : "Opponent turn";
        }

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

        private void GetShotResult(object sender, WebSocketContextEventArgs e) {
            if(!(e.message.requestType == RequestType.ShotResult)) return;
            Dictionary<string, JObject> data = Message.DeserializeData(e.message);
            ShotResult result = data["shotResult"].ToObject<ShotResult>();
            PlayerBoard board = !playerTurn ? this.board : this.opponentBoard;
            Border[,] borders = !playerTurn ? this.playerBorders : this.opponentBorders;

            if(result.shotStatus == ShotStatus.Miss){
                hitInfo.Text = "Miss!";
                setTurnInfo(!playerTurn);
                ChangeCellColor(board, borders, Brushes.Gray, result.column + 1, result.row + 1, true);
                if(playerTurn) {
                    lockShooting = true;
                    playerTurn = false;
                } else {
                    lockShooting = false;
                    playerTurn = true;
                }

            } else {
                if(result.shotStatus == ShotStatus.Hit) {
                    hitInfo.Text = "Hit!";
                    board.Update(result.column, result.row, ShotStatus.Hit);
                    ChangeCellColor(board, borders, Brushes.Orange, result.column + 1, result.row + 1, true);
                } else if(result.shotStatus == ShotStatus.Destroyed) {
                    hitInfo.Text = "Destroyed!";
                    
                    List<Coords> destroyedShipCoords = board.GetShipCoords(result.column, result.row);
                    foreach (Coords coord in destroyedShipCoords)
                    {
                        board.Update(coord.Column, coord.Row, ShotStatus.Destroyed);
                        ChangeCellColor(board, borders, Brushes.OrangeRed, coord.Column + 1, coord.Row + 1, true);
                    }
                }
                if(playerTurn) {
                    lockShooting = false;
                }
            }

        }

        private void Shot(object sender, MouseEventArgs e) {
            e.Handled = true;
            if(!playerTurn || lockShooting) {
                return;
            }
            if(e.Source is Border) {
                Border br = (Border)e.Source;
                if(br.Background == Brushes.Gray || br.Background == Brushes.Orange) {
                    return;
                }
                if(Grid.GetRow(br) == 0 || Grid.GetColumn(br) == 0) {
                    return;
                }
                lockShooting = true;
                this.game.Shot(Grid.GetRow(br) - 1, Grid.GetColumn(br) - 1);
            }
        }
    }
}