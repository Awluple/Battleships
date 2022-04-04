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
        public ShipsPlacement(Game game) {
            InitializeComponent();
            this.game = game;
            this.CreateGrid();
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

        public void CreateGrid() {
            
            
            for (var row = 1; row != 11; row++)
            {             
                for (var column = 1; column != 11; column++){
                var myBorder = GetBorder(Brushes.LightBlue);
                Grid.SetRow(myBorder, row);
                Grid.SetColumn(myBorder, column);
                boardGrid.Children.Add(myBorder);
                }
            } 
            string[] columns = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};

            for (var column = 1; column != 11; column++)
            {             
                var myBorder = GetBorder(Brushes.BurlyWood);

                var text = GetTextBlock();
                text.Text = columns[column-1];
                
                myBorder.Child = text;
                Grid.SetRow(myBorder, 0);
                Grid.SetColumn(myBorder, column);
                boardGrid.Children.Add(myBorder);
            }

            for (var row = 1; row != 11; row++)
            {             
                var myBorder = GetBorder(Brushes.BurlyWood);

                var text = GetTextBlock();
                
                text.Text = (row).ToString();

                myBorder.Child = text;
                Grid.SetRow(myBorder, row);
                Grid.SetColumn(myBorder, 0);
                boardGrid.Children.Add(myBorder);
            } 
        }
    }
}
