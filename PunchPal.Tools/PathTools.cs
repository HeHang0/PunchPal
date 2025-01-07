using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PunchPal.Tools
{
    public static class PathTools
    {
        public static string AppDataPath
        {
            get
            {
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appName = System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Name?.ToString() ?? "PunchPal";
                string dataPath = Path.Combine(roming, appName);
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
                return dataPath;
            }
        }

        public static string CookiePath => Path.Combine(AppDataPath, "token");

        public static string SettingPath => Path.Combine(AppDataPath, "setting");

        public static string StartupPath => Path.Combine(AppDataPath, "startup");

        public static string LogPath => Path.Combine(AppDataPath, "log.txt");

        public static string WinSize => Path.Combine(AppDataPath, "win_size");

        public static string Update => Path.Combine(AppDataPath, "update");

        public static string SqlitePath
        {
            get
            {
                var sqlitePath = Path.Combine(AppDataPath, "sqlite");
                if (!Directory.Exists(sqlitePath)) Directory.CreateDirectory(sqlitePath);
                return sqlitePath;
            }
        }
    }
}
