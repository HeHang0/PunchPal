using System;
using System.IO;

namespace PunchPal.Tools
{
    public static class PathTools
    {
        public static string AppName = "PunchPal";
        private static string _appDataPath = null;
        public static string RomingPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppDataPath
        {
            get
            {
                if (_appDataPath != null) return _appDataPath;
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataPath = Path.Combine(roming, AppName);
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
                _appDataPath = dataPath;
                return dataPath;
            }
        }

        public static string SettingPath => Path.Combine(AppDataPath, "setting");

        public static string DataSourcePath => Path.Combine(AppDataPath, "data_source");

        public static string DatabasePath = Path.Combine(PathTools.AppDataPath, "data.sqlite3");

        public static string WinSize => Path.Combine(AppDataPath, "win_size");

        public static string Update => Path.Combine(AppDataPath, "update");

        public static string CookiePath => Path.Combine(AppDataPath, "cookie");

        public static string PreDataPath => Path.Combine(AppDataPath, "pre_data");

        public static string SqlitePath
        {
            get
            {
                var sqlitePath = Path.Combine(AppDataPath, "sqlite");
                if (!Directory.Exists(sqlitePath)) Directory.CreateDirectory(sqlitePath);
                return sqlitePath;
            }
        }

        private static string _skiaSharpPath = null;
        public static string SkiaSharpPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_skiaSharpPath))
                {
                    return _skiaSharpPath;
                }
                var skiaPath = Path.Combine(AppDataPath, "skia");
                if (!Directory.Exists(skiaPath)) Directory.CreateDirectory(skiaPath);
                skiaPath = Path.Combine(skiaPath, "x64");
                if (!Directory.Exists(skiaPath)) Directory.CreateDirectory(skiaPath);
                _skiaSharpPath = Path.Combine(skiaPath, "libSkiaSharp.dll");
                return _skiaSharpPath;
            }
        }

        private static string _harfBuzzSharpPath = null;
        public static string HarfBuzzSharpPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_harfBuzzSharpPath))
                {
                    return _harfBuzzSharpPath;
                }
                var skiaPath = Path.Combine(AppDataPath, "skia");
                if (!Directory.Exists(skiaPath)) Directory.CreateDirectory(skiaPath);
                skiaPath = Path.Combine(skiaPath, "x64");
                if (!Directory.Exists(skiaPath)) Directory.CreateDirectory(skiaPath);
                _harfBuzzSharpPath = Path.Combine(skiaPath, "libHarfBuzzSharp.dll");
                return _harfBuzzSharpPath;
            }
        }
    }
}
