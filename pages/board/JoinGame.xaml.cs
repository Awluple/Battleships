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
    /// <summary>Handles joining to games from GamesList page</summary>
    /// <param name="gameId">Id of a game to join</param>
    public partial class JoinGame : Page
    {
        private Game game;
        private int gameId;
        public JoinGame(int gameId)
        {
            InitializeComponent();
            this.gameId = gameId;
            Loaded += Join;
        }

        private async void Join(object sender, RoutedEventArgs e) {
            this.game = new Game(this.gameId);
            if(await game.Connect() == false) {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow?.SessionError();
                return;
            }
            Game.WebSocketMessage += this.GetConfirmation;
            this.game.JoinGame();
        }
        /// <summary>Checks if connection to the server and joining to the game were successful, then redirects to the ShipPlacement page</summary>
        private void GetConfirmation(object sender, WebSocketContextEventArgs e) {
            if(!(e.message.requestType == RequestType.JoinConfirmation)) return;

            Game.WebSocketMessage -= this.GetConfirmation;
            Dictionary<string, JObject> data = Message.DeserializeData(e.message);
            JoinConfirmation confirmation = data["confirmation"].ToObject<JoinConfirmation>();
            
            if(confirmation.succeed) {
                var page = new ShipsPlacement(this.game);
                NavigationService.Navigate(page);
            } else {
                info.Text = $"Could not join the game. It is full or has been closed";
                backButton.Visibility = Visibility.Visible;
            }
        
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e) {
            Uri uri = new Uri("../views/menu/MainMenu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(uri);
        }
    }
}
