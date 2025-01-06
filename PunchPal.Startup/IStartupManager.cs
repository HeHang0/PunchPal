namespace PunchPal.Startup
{
    public interface IStartupManager
    {
        void EnableStartup(string appName, string appPath);
        void DisableStartup(string appName);
        bool IsStartupEnabled(string appName);
    }
}
