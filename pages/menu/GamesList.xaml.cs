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
using Newtonsoft.Json.Linq;
using BattleshipsShared.Models;
using BattleshipsShared.Communication;

using Battleships.Board;

namespace Battleships.Menu
{

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

        public async void LoadGames(object sender = null, RoutedEventArgs e = null) {
            infoHeader.Text = "Loading games...";
            gamesListGrid.Children.Clear(); // clear old games list
            Dictionary<string, GameInfo[]> games = await this.GetGamesAsync();

            if(!games.ContainsKey("games")) return;

            infoHeader.Text = "Games:";

            List<RowDefinition> rows = new List<RowDefinition>();

            int rowIndex = 0;

            foreach (var game in games["games"])
            {
                if(game.players == 2) continue; // ignore full games

                var row = new RowDefinition();

                GridLengthConverter gridLengthConverter = new GridLengthConverter();
                row.Height = (GridLength)gridLengthConverter.ConvertFromString("auto");
                rows.Add(row);
                gamesListGrid.RowDefinitions.Add(row);


                TextBlock gameInfo = new TextBlock();
                gameInfo.Text = $"Game #{game.id}";
                gameInfo.FontSize = 24;
                gameInfo.FontWeight = FontWeights.Bold;
                gameInfo.TextAlignment = TextAlignment.Center;
                gameInfo.Margin = new Thickness{ Bottom = 10, Top = 10 };
                
                Grid.SetRow(gameInfo, rowIndex);
                Grid.SetColumn(gameInfo, 0);



                Button joinButton = new Button();
                joinButton.Tag = game.id;
                joinButton.Click += this.Join;
                joinButton.Style = (Style)FindResource("Menu");
                joinButton.Content = "Join";
                joinButton.HorizontalAlignment = HorizontalAlignment.Center;
                joinButton.Padding = new Thickness{ Right = 10, Left = 10 };
                joinButton.Margin = new Thickness{ Bottom = 5, Top = 5 };

                Grid.SetRow(joinButton, rowIndex);
                Grid.SetColumn(joinButton, 1);

                gamesListGrid.Children.Add(gameInfo);
                gamesListGrid.Children.Add(joinButton);
                rowIndex++;
            }
        }
        public void Join(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            var page = new JoinGame((int)button.Tag);
            NavigationService.Navigate(page);
        }

        private async Task<Dictionary<string, GameInfo[]>> GetGamesAsync() {

            List<(int, int)> games = new List<(int, int)>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + Settings.serverUri);
        
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
            catch
            {
                infoHeader.Text = "Connection error";
                return new Dictionary<string, GameInfo[]>();
            }
            
        }

    }
}
