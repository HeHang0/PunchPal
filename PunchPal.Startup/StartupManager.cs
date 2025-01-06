using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PunchPal.Startup
{
    public class StartupManager
    {
        private IStartupManager manager;
        private string appName;
        private string appPath;
        public StartupManager(string appName, string appPath)
        {
            this.appName = appName;
            this.appPath = appPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                manager = new WindowsStartupManager();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                manager = new MacOSStartupManager();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                manager = new LinuxStartupManager();
            }
        }
        public void DisableStartup()
        {
            manager?.DisableStartup(appName);
        }

        public void EnableStartup()
        {
            manager?.EnableStartup(appName, appPath);
        }

        public bool IsStartupEnabled()
        {
            return manager?.IsStartupEnabled(appName) ?? false;
        }
    }
}
