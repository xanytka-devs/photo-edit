using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace XPhotos {

    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    
    public partial class App : Application {

        public static string CurrentDir { get; private set; }
        public static string LogsDir { get; private set; }
        public static string ConfigFile { get; private set; }

        protected override void OnStartup(StartupEventArgs e) {

            CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
            ConfigFile = Path.Combine(CurrentDir, "app.cfg");
            if(!File.Exists(ConfigFile)) File.Create(ConfigFile);
            LogsDir = Path.Combine(CurrentDir, "CrashLogs");
            if (!Directory.Exists(LogsDir)) Directory.CreateDirectory(LogsDir);

            base.OnStartup(e);
        }

    }
}
