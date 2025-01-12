using System.Runtime.InteropServices;

namespace PunchPal.Startup
{
    public class StartupManager
    {
        private readonly IStartupManager manager;
        private readonly string appName;
        private readonly string appPath;
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
