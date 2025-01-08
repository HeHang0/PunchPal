using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using PunchPal.Tools;
using Newtonsoft.Json;
using PunchPal.Startup;

namespace PunchPal.Core.ViewModels
{
    public class Settings: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly Settings _settings;

        static Settings()
        {
            try
            {
                _settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PathTools.SettingPath));
            }
            catch (Exception)
            {
            }
            _startupManager = new StartupManager("PunchPal", System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (_settings == null) _settings = new Settings();
            if (_settings.IsStartupEnabled)
            {
                SetStartup(true);
            }
        }

        public bool IsStartupEnabled
        {
            get => _startupManager?.IsStartupEnabled() ?? false;
            set
            {
                SetStartup(value);
                OnPropertyChanged(nameof(IsStartupEnabled));
            }
        }

        private static StartupManager _startupManager;
        private static void SetStartup(bool startup)
        {
            if (startup)
            {
                _startupManager?.EnableStartup();
            }
            else
            {

                _startupManager?.DisableStartup();
            }
        }

        private Settings()
        {
        }

        public static Settings Load()
        {
            return _settings;
        }

        public override string ToString()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private CancellationTokenSource saveCts;

        public void Save()
        {
            saveCts?.Cancel();
            saveCts = new CancellationTokenSource();
            _ = SaveReal(saveCts.Token);
        }

        private async Task SaveReal(CancellationToken? token = null)
        {
            try
            {
                if (token != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token.Value);
                }
                File.WriteAllText(PathTools.SettingPath, ToString());
            }
            catch (Exception)
            {
                // ignore
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
