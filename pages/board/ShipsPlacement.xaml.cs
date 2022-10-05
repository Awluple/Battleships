using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
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

    public partial class ShipsPlacement : BoardPage
    {
        private ShipOrientation shipOrientation = ShipOrientation.Vertical;
        private ShipsClasses selectedShip = ShipsClasses.Carrier;
        private int shipsLeft = 7;
        private bool isStartingPlayer = false;
        private Border[,] borders;

        private DispatcherTimer dispatcherTimer;
        public ShipsPlacement(Game game) : base(game) {
            InitializeComponent();
            if(NavigationService == null) {
                Loaded += onLoad;
            }
            Game.WebSocketMessage += this.EnemyDisconnected;

            this.game = game;
            this.board = new PlayerBoard();
            this.borders = this.CreateGrid(boardGrid);
            this.AddEvents();
            this.DataContext = board;
            this.Overlay_Disconnected = Disconnected_Overlay;
        }

        private void onLoad(object sender, RoutedEventArgs e) {
            // this.NavigationService.Navigating += this.NavigationService_WebSocketDisconnect;
        }

        private void RepaintArea(Border br, Brush color) {
            int maxColumn = this.shipOrientation == ShipOrientation.Vertical ? (int)this.selectedShip + 1 : 2;
            int maxRow = this.shipOrientation == ShipOrientation.Horizontal ? (int)this.selectedShip + 1 : 2;

            for (int column = -1; column < maxColumn; column++) {
                for (var row = -1; row < maxRow; row++) {
                    ChangeCellColor(this.board, borders, color, Grid.GetColumn(br) + column, Grid.GetRow(br) + row);
                }
            }
        }

        private void over(object sender, MouseEventArgs e) {
            e.Handled = true;
            if(this.board.shipsLeft[selectedShip] == 0) {
                return;
            }
            if(e.Source is Border) {
                Border br = (Border)e.Source;
                bool placeOk = this.board.CheckPlacement(selectedShip, shipOrientation, Grid.GetColumn(br) - 1, Grid.GetRow(br) - 1);

                Brush shipColor = Brushes.GreenYellow;
                Brush areaColor = Brushes.YellowGreen;
                if(!placeOk) {
                    shipColor = Brushes.OrangeRed;
                    areaColor = Brushes.Orange;
                }
                // Paint the area
                RepaintArea(br, areaColor);

                // Paint the ship
                for (int i = 0; i < (int)this.selectedShip; i++) {
                    if(this.shipOrientation == ShipOrientation.Vertical) {
                        ChangeCellColor(this.board,borders, shipColor, Grid.GetColumn(br) + i, Grid.GetRow(br));
                    } else {
                        ChangeCellColor(this.board,borders, shipColor, Grid.GetColumn(br), Grid.GetRow(br) + i);
                    }
                }
            }
        }

        private void leaves(object sender, MouseEventArgs e) {
            e.Handled = true;
            if(e.Source is Border) {
                Border br = (Border)e.Source;
                RepaintArea(br, Brushes.LightBlue);
            }
        }

        private void PlaceShip(object sender, MouseEventArgs e) {
            e.Handled = true;
            if(e.Source is Border) {
                Border br = (Border)e.Source;
                bool placeOk = this.board.PlaceShip(this.selectedShip, this.shipOrientation,Grid.GetColumn(br) - 1, Grid.GetRow(br) - 1);
                RepaintArea(br, Brushes.LightBlue);
                if(placeOk) {
                    this.shipsLeft--;
                }

                if(this.shipsLeft == 0) {
                    Game.WebSocketMessage += this.StartGame;
                    game.SendBoard(board);
                    this.shipsLeft = -1;

                    orientationImage.Visibility = Visibility.Hidden;
                    orientationText.Visibility = Visibility.Hidden;
                    waitingText.Visibility = Visibility.Visible;
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(UpdateWaiting);
                    dispatcherTimer.Interval = new TimeSpan(0,0,0,0,800);
                    dispatcherTimer.Start();
                }
            };
        }
        private void UpdateWaiting(object sender, EventArgs e)
        {
            if(waitingText.Text.Length >= 27) {
                waitingText.Text = "Waiting for the opponent.";
            } else {
                waitingText.Text = waitingText.Text + ".";
            }
        }

        private void StartGame(object sender, WebSocketContextEventArgs e)
        {
            if(e.message.requestType != RequestType.GameReady) {
                return;
            }
            Game.WebSocketMessage -= this.StartGame;
            dispatcherTimer.Stop();
            JObject obj = (JObject)e.message.data;

            Dictionary<string, object> data = obj.ToObject<Dictionary<string, object>>();
            this.isStartingPlayer = Int32.Parse(data["startingPlayer"].ToString()) == Settings.userId;

            if(NavigationService == null) {
                Loaded += redirect;
            } else {
                redirect();
            }  
        }

        private void redirect(object sender, RoutedEventArgs e) {
            var page = new MainBoard(this.game, this.board, this.isStartingPlayer);
            NavigationService.Navigate(page);
        }
        private void redirect() {
            var page = new MainBoard(this.game, this.board, this.isStartingPlayer);
            NavigationService.Navigate(page);
        }

        private void NavigationService_WebSocketDisconnect(object sender, NavigatingCancelEventArgs e)
        {
            Debug.WriteLine("NAVI!");
            Game.CloseConnection();
        }

         private void ChangeOrientation(object sender, MouseEventArgs e) {
            this.Rotate();
         }

         private void ChangeOrientation() {
            this.Rotate();
         }

        private void Rotate() {
            if(this.shipOrientation == ShipOrientation.Horizontal) {
                this.shipOrientation = ShipOrientation.Vertical;
                RotateTransform rotateTransform = new RotateTransform(90);
                rotateTransform.CenterX = 40;
                rotateTransform.CenterY = 40;
                orientationImage.RenderTransform = rotateTransform;
            } else {
                this.shipOrientation = ShipOrientation.Horizontal;
                RotateTransform rotateTransform = new RotateTransform(0);
                rotateTransform.CenterX = 40;
                rotateTransform.CenterY = 40;
                orientationImage.RenderTransform = rotateTransform;
            }
        }

        private void SelectShip(ShipsClasses newShip) {
            if(this.board.shipsLeft[newShip] == 0) {
                return;
            }
            // remove opacity from the currently selected ship
            string name = this.selectedShip.ToString();
            var ship = (Image)this.FindName(name);
            ship.Opacity = 1;

            ship = (Image)this.FindName(newShip.ToString());
            ship.Opacity = 0.6;
            this.selectedShip = newShip;
        }

        private void changeShip(object sender, MouseEventArgs e) {
            var text = sender as Image;
            var ship = (ShipsClasses)Int32.Parse(text.Tag.ToString());
            this.SelectShip(ship);
        }

        private void AddEvents() {
            foreach (Border item in boardGrid.Children)
            {
                item.MouseEnter += over;
                item.MouseLeave += leaves;
                item.MouseLeftButtonDown += PlaceShip;
            }
        }
}
}