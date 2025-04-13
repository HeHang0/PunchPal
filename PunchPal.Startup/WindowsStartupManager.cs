using System;
using System.IO;

namespace PunchPal.Startup
{
    public class WindowsStartupManager : IStartupManager
    {
        public void EnableStartup(string appName, string appPath)
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string lnkPath = Path.Combine(startupFolderPath, Path.GetFileNameWithoutExtension(appName) + ".lnk");
            ShellLink.Shortcut.CreateShortcut(appPath, "--startup").WriteToFile(lnkPath);
        }

        public void DisableStartup(string appName)
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string lnkPath = Path.Combine(startupFolderPath, Path.GetFileNameWithoutExtension(appName) + ".lnk");
            var exists = File.Exists(lnkPath);
            if (exists)
            {
                File.Delete(lnkPath);
            }
        }

        public bool IsStartupEnabled(string appName)
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string lnkPath = Path.Combine(startupFolderPath, Path.GetFileNameWithoutExtension(appName) + ".lnk");
            return File.Exists(lnkPath);
        }
    }
}
