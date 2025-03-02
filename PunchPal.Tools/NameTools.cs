using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PunchPal.Tools
{
    public class NameTools
    {
        public static string NamedPipe => "_PUNCH_PAL_PIPE";
        public static string AppMutex => "_PUNCH_PAL_MUTEX";
        public static string AppName => "PunchPal";
        public static string AppTitle => "工时助手";
        public static string AppSubTitle => "高效的管理工作";

        private static string _appVersion = null;
        public static string AppVersion
        {
            get
            {
                if (_appVersion != null)
                {
                    return _appVersion;
                }
                try
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName);
                    var versionArray = fileVersionInfo.FileVersion.Split('.');
                    _appVersion = "v" + string.Join(".", versionArray.Take(versionArray.Length >= 4 && !string.IsNullOrWhiteSpace(versionArray[3]) && versionArray[3] != "0" ? 4 : 3));
                }
                catch (Exception)
                {
                    _appVersion = string.Empty;
                }
                return _appVersion;
            }
        }
    }
}
