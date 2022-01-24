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

namespace Battleships
{
    public partial class GamesList : Page
    {
        public GamesList()
        {
            InitializeComponent();
            List<RowDefinition> rows = new List<RowDefinition>();
            rows.Add(new RowDefinition());
            rows.Add(new RowDefinition());

            foreach (var row in rows)
            {
                row.MaxHeight = 50;
                gamesListGrid.RowDefinitions.Add(row);
            }

            TextBlock txt1 = new TextBlock();
            txt1.Text = "Game 1";
            txt1.FontSize = 12;
            txt1.FontWeight = FontWeights.Bold;
            Grid.SetRow(txt1, 0);
            Grid.SetColumn(txt1, 1);

            TextBlock txt2 = new TextBlock();
            txt2.Text = "Game 2";
            txt2.FontSize = 12;
            txt2.FontWeight = FontWeights.Bold;
            Grid.SetRow(txt2, 1);
            Grid.SetColumn(txt2, 1);

            gamesListGrid.Children.Add(txt1);
            gamesListGrid.Children.Add(txt2);

        }
    }
}
