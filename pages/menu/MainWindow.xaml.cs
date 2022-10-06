using System;
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

using System.ComponentModel;
using Battleships.Board;

namespace Battleships
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            Game.WebSocketError += ConnectionError;
        }

        public void ChangeView(Page view)
        {
            MainContainer.NavigationService.Navigate(view);
        }

        public void ConnectionError(object sender, WebSocketErrorContextEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
        }
        public void hyperlink_Reconnect(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Hidden;
           Uri uri = new Uri("../views/menu/StartPage.xaml", UriKind.Relative);
           Settings.userId = 0;
           this.MainContainer.NavigationService.Navigate(uri);
        }
    }
}
