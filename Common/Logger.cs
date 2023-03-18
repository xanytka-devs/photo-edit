using System;
using System.IO;

namespace XPhotos.Common {

    public class Logger {

        static string logged = string.Empty;

        /// <summary>
        /// This function will add new text to current log.
        /// </summary>
        /// <param name="msg">Message to log.</param>
        public static void Log(string msg) => logged += $"\n {msg}";

        /// <summary>
        /// This function will save all logged text in a log in Application's log directory.
        /// </summary>
        public static void PushLog() {
            string pth = Path.Combine(App.LogsDir, DateTime.Today.ToString("O").Split('T')[0] + ".log");
            File.WriteAllText(pth, logged);
        }

    }

}
