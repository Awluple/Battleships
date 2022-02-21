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

namespace Battleships
{

    public struct GameInfo
    {
        public GameInfo(int id, int players)
        {
            this.id = id;
            this.players = players;
        }
    public int id { get; }
    public int players { get; }
    }

    public partial class GamesList : Page
    {
        public GamesList()
        {
            InitializeComponent();
            this.LoadGames();
        }

        public void hyperlink_MainMenu(object sender, RoutedEventArgs e) {
           Uri uri = new Uri("../views/MainMenu.xaml", UriKind.Relative);
           this.NavigationService.Navigate(uri);
        }

        public async void LoadGames(object sender = null, RoutedEventArgs e = null) {
            gamesListGrid.Children.Clear(); // clear old games list
            Dictionary<string, GameInfo[]> games = await this.GetGamesAsync();

            List<RowDefinition> rows = new List<RowDefinition>();

            int rowIndex = 0;

            foreach (var game in games["games"])
            {
                if(game.players == 2) continue; // ignore full games

                var row = new RowDefinition();

                GridLengthConverter gridLengthConverter = new GridLengthConverter();
                row.Height = (GridLength)gridLengthConverter.ConvertFromString("50");
                rows.Add(row);
                gamesListGrid.RowDefinitions.Add(row);


                TextBlock txt = new TextBlock();
                txt.Text = $"Game #{game.id}";
                txt.FontSize = 12;
                txt.FontWeight = FontWeights.Bold;
                Grid.SetRow(txt, rowIndex);
                Grid.SetColumn(txt, 1);

                gamesListGrid.Children.Add(txt);
                rowIndex++;
            }
        }


        public async Task<Dictionary<string, GameInfo[]>> GetGamesAsync() {

            List<(int, int)> games = new List<(int, int)>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.serverUri);
        
            using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync()){
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, GameInfo[]>>(json);
            }
            }
        }

    }
}
