using System;
using System.IO;

namespace PunchPal.Startup
{
    public class LinuxStartupManager: IStartupManager
    {
        public void EnableStartup(string appName, string executablePath)
        {
            string autostartFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".config/autostart");

            Directory.CreateDirectory(autostartFolder);

            string desktopFilePath = Path.Combine(autostartFolder, $"{appName}.desktop");
            string desktopFileContent = $@"
[Desktop Entry]
Type=Application
Name={appName}
Exec={executablePath}
X-GNOME-Autostart-enabled=true";

            File.WriteAllText(desktopFilePath, desktopFileContent);
        }

        public void DisableStartup(string appName)
        {
            string desktopFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".config/autostart",
                $"{appName}.desktop");

            if (File.Exists(desktopFilePath))
            {
                File.Delete(desktopFilePath);
            }
        }

        public bool IsStartupEnabled(string appName)
        {
            string autostartFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".config/autostart");

            string desktopFilePath = Path.Combine(autostartFolder, $"{appName}.desktop");
            return File.Exists(desktopFilePath);
        }
    }
}
