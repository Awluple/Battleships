using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Windows;
using System.Diagnostics;

using Newtonsoft.Json;

namespace Battleships
{
    static class Settings {
        public static readonly string serverUri = "127.0.0.1:7850/";
        public static int userId = 0;
    }
    
    public partial class App : Application
    {
    }
}
