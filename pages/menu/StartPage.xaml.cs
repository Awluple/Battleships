using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Windows;
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

namespace Battleships.Menu
{
    public partial class StartPage : Page
    {
        public StartPage() {
            InitializeComponent();
            GetUserId();
        }

        private void ShowConnectionError(string msg) {
            info.Text = msg;
            reconnectButton.Visibility = Visibility.Visible;
        }

        public void Reconnect(object sender, RoutedEventArgs e) {
            info.Text = "Connecting...";
            reconnectButton.Visibility = Visibility.Hidden;
            GetUserId();
        }
        /// <summary>Gets an ID for the user from the server, if server is not avaliable shows an error on the UI</summary>
        async private void GetUserId() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + Settings.serverUri + "userid");
            request.Headers["sessionId"] = Settings.sessionId;
        
            try
            {
                using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    string json = await reader.ReadToEndAsync();
                    Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    Settings.userId = Int32.Parse(data["id"]);
                    Settings.sessionId = data["sessionId"];
                    Uri uri = new Uri("../views/menu/MainMenu.xaml", UriKind.Relative);
                    this.NavigationService.Navigate(uri);
            }
            }
            catch(WebException e)
            {
                ShowConnectionError("Could not connect to the server");
                Debug.WriteLine(e.Message);
            }
        }
    }
}
