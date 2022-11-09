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
        public static readonly string serverUri = System.IO.File.ReadAllText(new Uri(@".\server_address.txt", UriKind.Relative).ToString()) + "/";
        public static int userId = 0;
        public static string sessionId = "";
    }
    
    public partial class App : Application
    {
    }
}
