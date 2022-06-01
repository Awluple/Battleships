using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
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

    public partial class ShipsPlacement : Page
    {
        private Game game;
        private GameBoard board;
        private ShipOrientation shipOrientation = ShipOrientation.Vertical;
        private ShipsClasses selectedShip = ShipsClasses.Carrier;
        private int shipsLeft = 7;

        Border[,] borders = new Border[10,10];
        public ShipsPlacement(Game game) {
            InitializeComponent();
            this.game = game;
            this.board = new GameBoard();
            this.CreateGrid();
            this.DataContext = board;
        }

        private Border GetBorder(Brush color) {
            var myBorder = new Border();
            myBorder.Background = color;
            myBorder.BorderBrush = Brushes.Black;
            myBorder.BorderThickness = new Thickness(0.5);
            return myBorder;
        }

        private TextBlock GetTextBlock() {
            var text = new TextBlock();
            text.VerticalAlignment = VerticalAlignment.Center;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.FontSize = 18;
            text.FontWeight = FontWeights.Bold;
            return text;
        }

        private void ChangeCellColor(Brush color, int column, int row) {
                if(row <= 0 || column <= 0 || column >= 11 || row >= 11) { // ignore label cells
                    return;
                }
                if(this.board.IsOccupied(column- 1, row - 1)) {
                    return;
                }
                borders[column - 1, row - 1].Background = color;
        }

        private void RepaintArea(Border br, Brush color) {
            int maxColumn = this.shipOrientation == ShipOrientation.Vertical ? (int)this.selectedShip + 1 : 2;
            int maxRow = this.shipOrientation == ShipOrientation.Horizontal ? (int)this.selectedShip + 1 : 2;

            for (int column = -1; column < maxColumn; column++) {
                for (var row = -1; row < maxRow; row++) {
                    ChangeCellColor(color, Grid.GetColumn(br) + column, Grid.GetRow(br) + row);
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
                        ChangeCellColor(shipColor, Grid.GetColumn(br) + i, Grid.GetRow(br));
                    } else {
                        ChangeCellColor(shipColor, Grid.GetColumn(br), Grid.GetRow(br) + i);
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
                    game.SendBoard(board);
                    this.shipsLeft = -1;
                }
            };
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

        public void CreateGrid() {
            for (var row = 1; row != 11; row++) {             
                for (var column = 1; column != 11; column++){
                var myBorder = GetBorder(Brushes.LightBlue);
                myBorder.Cursor = Cursors.Hand;
                Grid.SetRow(myBorder, row);
                Grid.SetColumn(myBorder, column);
                borders[column -1 , row - 1] = myBorder;
                boardGrid.Children.Add(myBorder);
                }
            } 
            string[] columns = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};

            for (var column = 1; column != 11; column++) {             
                var myBorder = GetBorder(Brushes.BurlyWood);

                var text = GetTextBlock();
                text.Text = columns[column-1];
                
                myBorder.Child = text;
                Grid.SetRow(myBorder, 0);
                Grid.SetColumn(myBorder, column);
                boardGrid.Children.Add(myBorder);
            }

            for (var row = 1; row != 11; row++) {             
                var myBorder = GetBorder(Brushes.BurlyWood);

                var text = GetTextBlock();
                
                text.Text = (row).ToString();

                myBorder.Child = text;
                Grid.SetRow(myBorder, row);
                Grid.SetColumn(myBorder, 0);
                boardGrid.Children.Add(myBorder);
            }

            foreach (Border item in boardGrid.Children)
            {
                item.MouseEnter += over;
                item.MouseLeave += leaves;
                item.MouseLeftButtonDown += PlaceShip;
            }
        }
    }
}
