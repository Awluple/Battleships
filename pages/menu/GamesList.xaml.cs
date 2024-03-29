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

using Newtonsoft.Json;
using BattleshipsShared.Models;

using Battleships.Board;

namespace Battleships.Menu
{
    /// <summary>Controls the games list page</summary>
    public partial class GamesList : Page
    {
        public GamesList()
        {
            InitializeComponent();
            this.LoadGames();
        }

        public void hyperlink_MainMenu(object sender, RoutedEventArgs e) {
           Uri uri = new Uri("../views/menu/MainMenu.xaml", UriKind.Relative);
           this.NavigationService.Navigate(uri);
        }
        /// <summary>Gets the games from the server and shows them on the UI</summary>
        public async void LoadGames(object sender = null, RoutedEventArgs e = null) {
            infoHeader.Text = "Loading games...";
            gamesListGrid.Children.Clear(); // clear old games list
            Dictionary<string, GameInfo[]> games = await this.GetGamesAsync();

            if(!games.ContainsKey("games")) {
                return;
            };
 
            if(games["games"].Count() == 0) {
                infoHeader.Text = "NO GAMES FOUND";
                return;
            }

            List<RowDefinition> rows = new List<RowDefinition>();

            int rowIndex = 0;

            foreach (var game in games["games"])
            {
                if(game.players == 2) continue; // ignore full games

                var row = new RowDefinition();

                // Creating new row in the grid and setting all the information about games
                GridLengthConverter gridLengthConverter = new GridLengthConverter();
                row.Height = (GridLength)gridLengthConverter.ConvertFromString("auto");
                rows.Add(row);
                gamesListGrid.RowDefinitions.Add(row);

                TextBlock gameInfo = new TextBlock();
                gameInfo.Style = (Style)this.Resources["gameId"];
                gameInfo.Text = $"Game #{game.id}";
                
                Grid.SetRow(gameInfo, rowIndex);
                Grid.SetColumn(gameInfo, 0);


                Button joinButton = new Button();
                joinButton.Tag = game.id;
                joinButton.Content = "JOIN";
                joinButton.Click += this.Join;
                joinButton.Style = (Style)this.Resources["joinButton"];

                Grid.SetRow(joinButton, rowIndex);
                Grid.SetColumn(joinButton, 1);

                gamesListGrid.Children.Add(gameInfo);
                gamesListGrid.Children.Add(joinButton);
                rowIndex++;
            }

            if(rowIndex == 0) {
                infoHeader.Text = "NO GAMES FOUND";
            } else {
                infoHeader.Text = "GAMES:";
            }
        }
        public void Join(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            var page = new JoinGame((int)button.Tag);
            NavigationService.Navigate(page);
        }
        /// <summary>Creates http request to the server</summary>
        /// <returns>Dictionary with list of games</returns>
        private async Task<Dictionary<string, GameInfo[]>> GetGamesAsync() {

            List<(int, int)> games = new List<(int, int)>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + Settings.serverUri);
            request.Headers["sessionId"] = Settings.sessionId;
        
            try
            {
                using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    string json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<Dictionary<string, GameInfo[]>>(json);
            }
            }
            catch(System.Net.WebException ex) {
                if (ex.Status == WebExceptionStatus.ProtocolError) // get new user id after server restart
                {
                    HttpWebResponse res = (HttpWebResponse) ex.Response;
                    if((int)res.StatusCode == 403) {
                        var mainWindow = (MainWindow)Application.Current.MainWindow;
                        mainWindow?.SessionError();
                    }
                }
                return new Dictionary<string, GameInfo[]>();
            }
            catch
            {
                infoHeader.Text = "Connection error";
                return new Dictionary<string, GameInfo[]>();
            }
            
        }

    }
}
