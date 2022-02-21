using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Battleships
{
    static class Settings {
        public static readonly string serverUri = "http://127.0.0.1:7850/";
    }
    public partial class App : Application
    {
    }
}
