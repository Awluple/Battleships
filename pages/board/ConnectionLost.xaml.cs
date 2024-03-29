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

using Battleships.Menu;

namespace Battleships.Board
{
    /// <summary>Overlay showing up when connection with the server has been lost</summary>
    public partial class ConnectionLost : Page
    {
        public ConnectionLost()
        {
            InitializeComponent();
        }

        public void BackToMainMenu(object sender, RoutedEventArgs e) {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow?.ChangeView(new MainMenu());
            Application.Current.MainWindow.Height = 970;
        }
    }
}
