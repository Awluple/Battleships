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

using Newtonsoft.Json;
using BattleshipsShared.Communication;
using BattleshipsShared.Models;

using Battleships.Board;

namespace Battleships.Menu
{

    public struct GameId
    {

        public GameId(int id) {
            this.id = id;
        }

        public int id { get; set; }
    }

    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();

            userId.Text = "Admiral #" + Settings.userId.ToString();
        }

        public void hyperlink_Join(object sender, RoutedEventArgs e)
        {
           Uri uri = new Uri("../views/menu/GamesList.xaml", UriKind.Relative);
           this.NavigationService.Navigate(uri);
        }

        public void hyperlink_CreateGame(object sender, RoutedEventArgs e) {
            Uri uri = new Uri("../views/board/CreateGame.xaml", UriKind.Relative);
           this.NavigationService.Navigate(uri);
        }
    }
}
