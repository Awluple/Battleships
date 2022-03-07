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

using Battleships.Board;

namespace Battleships.Menu
{
    public struct User
    {

        public User(string username) {
            this.username = username;
        }

        public string username { get; set; }
    }

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

            var board = new GameBoard();

            board.PlaceShip((4, 3), (1,1));
            Console.WriteLine(board.IsAvialiavle(7, 4));
        }

        public void hyperlink_Click(object sender, RoutedEventArgs e)
        {
           Uri uri = new Uri("../views/GamesList.xaml", UriKind.Relative);
           this.NavigationService.Navigate(uri);
        }

        public async void CreateGame(object sender, RoutedEventArgs e) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.serverUri);
            request.Method = "POST";
            request.ContentType = "application/json";
            using(Stream stream = request.GetRequestStream())
            using(StreamWriter writer = new StreamWriter(stream))
            {
                string serialized = JsonConvert.SerializeObject(new User("test"));
                await writer.WriteAsync(serialized);
            }
        
            try
            {
                using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    string data = await reader.ReadToEndAsync();
                    GameId gameId = JsonConvert.DeserializeObject<GameId>(data);
            }
            }
            catch
            {

            }
            this.JoinGame(1);
        }
        

        private void JoinGame(int gameId) {
            Game game = new Game("Test");
        }
    }
}
