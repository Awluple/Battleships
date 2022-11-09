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

using BattleshipsShared.Communication;

namespace Battleships.Board
{
    /// <summary>Base class for pages that show board on the UI</summary>
    /// <param name="game">Currenty connected game</param>
    public partial class BoardPage : Page
    {
        protected virtual Game game {get; set;}
        protected virtual PlayerBoard board {get; set;}
        public virtual Frame Overlay_Disconnected {get; set;}
        public BoardPage(Game game) {
            this.game = game;
        }

        /// <summary>Changes a cell color on selected position</summary>
        /// <param name="playerBoard">Board object to manipulate</param>
        /// <param name="borders">List of UI board borders </param>
        /// <param name="color">The color to paint the cell</param>
        /// <param name="column">Column number on the UI</param>
        /// <param name="row">Row number on the UI</param>
        /// <param name="ignoreOccupation">If false, only empty cells will be painted</param>

        protected void ChangeCellColor(PlayerBoard playerBoard, Border[,] borders, Brush color, int column, int row, bool ignoreOccupation = false) {
            if(row <= 0 || column <= 0 || column >= 11 || row >= 11) { // ignore label cells
                return;
            }
            if(!ignoreOccupation && playerBoard.getOccupation(column - 1, row - 1) != ShipStatus.Empty) {
                return;
            }
            borders[column - 1, row - 1].Background = color;
        }
        protected void EnemyDisconnected(object sender, WebSocketContextEventArgs e) {
            if(e.message.requestType != RequestType.OpponentConnectionLost) return;
            Game.WebSocketMessage -= this.EnemyDisconnected;
            Overlay_Disconnected.Visibility = Visibility.Visible;
        }
        /// <summary>Creates standard TextBlock object</summary>
        /// <returns>Standard TextBlock object</returns>
        protected TextBlock GetTextBlock() {
            var text = new TextBlock();
            text.VerticalAlignment = VerticalAlignment.Center;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.FontSize = 18;
            text.FontWeight = FontWeights.Bold;
            return text;
        }
        /// <summary>Creates standard Border object</summary>
        /// <param name="color">Color of the background</param>
        /// <returns>Standard Border object</returns>
        protected Border GetBorder(Brush color) {
            var myBorder = new Border();
            myBorder.Background = color;
            myBorder.BorderBrush = Brushes.Black;
            myBorder.BorderThickness = new Thickness(0.5);
            return myBorder;
        }
        /// <summary>Creates grid on the UI and sets rows and columns labels</summary>
        /// <param name="gridRef">Reference to the UI grid</param>
        /// <returns>List of references to the borders on the UI</returns>
        public Border[,] CreateGrid(Grid gridRef, Cursor cursor) {
            Border[,] borders = new Border[10,10];
            for (var row = 1; row != 11; row++) {             
                for (var column = 1; column != 11; column++){
                var myBorder = GetBorder(Brushes.LightBlue);
                myBorder.Cursor = cursor;
                Grid.SetRow(myBorder, row);
                Grid.SetColumn(myBorder, column);
                borders[column -1 , row - 1] = myBorder;
                gridRef.Children.Add(myBorder);
                }
            } 
            string[] columns = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};

            for (var column = 1; column != 11; column++) { // create column
                var myBorder = GetBorder(Brushes.BurlyWood);

                var text = GetTextBlock();
                text.Text = columns[column-1];
                
                myBorder.Child = text;
                Grid.SetRow(myBorder, 0);
                Grid.SetColumn(myBorder, column);
                gridRef.Children.Add(myBorder);
            }

            for (var row = 1; row != 11; row++) { // create rows        
                var myBorder = GetBorder(Brushes.BurlyWood);

                var text = GetTextBlock();
                
                text.Text = (row).ToString();

                myBorder.Child = text;
                Grid.SetRow(myBorder, row);
                Grid.SetColumn(myBorder, 0);
                gridRef.Children.Add(myBorder);
            }
            return borders;
        }
    }
}