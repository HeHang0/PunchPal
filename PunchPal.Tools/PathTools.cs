using System;
using System.IO;

namespace PunchPal.Tools
{
    public static class PathTools
    {
        public static string AppName = "PunchPal";
        public static string AppDataPath
        {
            get
            {
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataPath = Path.Combine(roming, AppName);
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
                return dataPath;
            }
        }

        public static string SettingPath => Path.Combine(AppDataPath, "setting");

        public static string DataSourcePath => Path.Combine(AppDataPath, "data_source");

        public static string DatabasePath = Path.Combine(PathTools.AppDataPath, "data.sqlite3");

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
