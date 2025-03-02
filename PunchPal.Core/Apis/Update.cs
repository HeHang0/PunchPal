using AutoUpdate.Core;
using PunchPal.Tools;
using System;

namespace PunchPal.Core.Apis
{
    public static class Update
    {
        public static event NewPackageCheckedEventHandler NewPackageChecked;

        public static readonly AutoUpdate.Core.AutoUpdate AutoUpdate;
        public static readonly GithubChecker GithubChecker;
        static Update()
        {
            GithubChecker = new GithubChecker("hehang0", NameTools.AppName, NameTools.AppName + ".exe", NameTools.AppVersion);
            AutoUpdate = new AutoUpdate.Core.AutoUpdate(new Options(GithubChecker, TimeSpan.FromHours(1)));
            AutoUpdate.NewPackageChecked += OnNewPackageChecked;
            AutoUpdate.Start();
        }

        private static void OnNewPackageChecked(AutoUpdate.Core.AutoUpdate sender, PackageCheckedEventArgs e)
        {
            NewPackageChecked?.Invoke(sender, e);
        }
    }
}
