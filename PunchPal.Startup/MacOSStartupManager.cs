using System;
using System.IO;

namespace PunchPal.Startup
{
    public class MacOSStartupManager : IStartupManager
    {
        public void EnableStartup(string appName, string executablePath)
        {
            string plistPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library/LaunchAgents",
                $"{appName}.plist");

            string plistContent = $@"
<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>{appName}</string>
    <key>ProgramArguments</key>
    <array>
        <string>{executablePath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>";

            File.WriteAllText(plistPath, plistContent);
        }

        public void DisableStartup(string appName)
        {
            string plistPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library/LaunchAgents",
                $"{appName}.plist");

            if (File.Exists(plistPath))
            {
                File.Delete(plistPath);
            }
        }

        public bool IsStartupEnabled(string appName)
        {
            string plistPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library/LaunchAgents",
                $"{appName}.plist");

            return File.Exists(plistPath);
        }
    }
}
