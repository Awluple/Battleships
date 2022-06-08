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


namespace Battleships.Board
{

    public partial class MainBoard : Page
    {
        private Game game;
        private GameBoard board;
        public MainBoard(Game game, GameBoard gameBoard) {
            InitializeComponent();
            this.game = game;
            this.board = gameBoard;
            // this.CreateGrid();gameBoard
            this.DataContext = board;
            Application.Current.MainWindow.Height = 1200;
        }
    }
}