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

using BattleshipsShared.Communication;
using BattleshipsShared.Models;

namespace Battleships.Board
{
    public struct GameId
    {

        public GameId(int id) {
            this.id = id;
        }

        public int id { get; set; }
    }
    
    public partial class CreateGame : Page
    {

        private Game game;

        public CreateGame() {
            InitializeComponent();
            this.SendCreateRequest();
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e) {
            Game.CloseConnection();
            Uri uri = new Uri("../views/menu/MainMenu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(uri);
        }

        public void Cancel(object sender, RoutedEventArgs e) {
            Game.CloseConnection();
            Uri uri = new Uri("../views/menu/MainMenu.xaml", UriKind.Relative);
            this.NavigationService.Navigate(uri);
        }

        public void ShowError(string error) {
            info.Text = error;
            backButton.Visibility = Visibility.Visible;
        }

        async public void SendCreateRequest() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + Settings.serverUri);
            request.Method = "POST";
            request.ContentType = "application/json";
            try
            {
                using(Stream stream = await request.GetRequestStreamAsync())
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    string serialized = JsonConvert.SerializeObject(new User("test"));
                    await writer.WriteAsync(serialized);
                }
            }
            catch (System.Net.WebException)
            {
                this.ShowError("Could not connect to the server");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
                this.ShowError("Could not create a game");
            }
        
            try
            {
                using(HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    string data = await reader.ReadToEndAsync();
                    GameId gameId = JsonConvert.DeserializeObject<GameId>(data);
                    
                    cancelButton.Visibility = Visibility.Visible;
                    this.JoinCreatedGame(gameId.id);
            }
            }
            catch(Exception err)
            {
                this.ShowError("Could not create a game");
            }
        }
        private void GetConfirmation(object sender, WebSocketContextEventArgs e) {
            if(!(e.message.requestType == RequestType.JoinConfirmation)) return;

            Game.WebSocketMessage -= this.GetConfirmation;
            Dictionary<string, JObject> data = Message.DeserializeData(e.message);
            JoinConfirmation confirmation = data["confirmation"].ToObject<JoinConfirmation>();
            if(confirmation.succeed) {
                info.Text = $"Waiting for an opponent to join in...";
                Game.WebSocketMessage += this.StartGame;
            } else {
                this.ShowError("Error. Unable to join the created game");
            }

        }

        public void StartGame(object sender, WebSocketContextEventArgs e) {
            if(!(e.message.requestType == RequestType.OpponentFound)) return;
            Game.WebSocketMessage -= this.StartGame;

            Dictionary<string, JObject> data = Message.DeserializeData(e.message);
            JoinConfirmation opponent = data["confirmation"].ToObject<JoinConfirmation>();

            
            if(NavigationService == null) {
                Loaded += redirect;
            } else {
                redirect();
            }
        }


        public void redirect(object sender, RoutedEventArgs e) {
            var page = new ShipsPlacement(this.game);
            NavigationService.Navigate(page);
        }

        public void redirect() {
            var page = new ShipsPlacement(this.game);
            NavigationService.Navigate(page);
        }

        private async void JoinCreatedGame(int gameId) {
            Game.WebSocketMessage += this.GetConfirmation;
            this.game = new Game(gameId);
            await game.Connect();
            game.JoinGame();
        }
    }
}
