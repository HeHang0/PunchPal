using Newtonsoft.Json;
using PunchPal.Startup;
using PunchPal.Tools;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PunchPal.Core.ViewModels
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly SettingsModel _settings;

        public SettingsData Data { get; set; } = new SettingsData();
        public SettingsCommon Common { get; set; } = new SettingsCommon();
        public SettingsNetwork Network { get; set; } = new SettingsNetwork();
        public SettingsPersonalize Personalize { get; set; } = new SettingsPersonalize();

        public enum PageType
        {
            Data,
            Common,
            Network,
            Personalize
        }

        [JsonIgnore]
        private PageType _currentSettingPage = PageType.Common;
        [JsonIgnore]
        public PageType CurrentSettingPage
        {
            get => _currentSettingPage;
            set
            {
                if (_currentSettingPage == value)
                {
                    return;
                }
                _currentSettingPage = value;
                OnPropertyChanged(nameof(CurrentSettingTitle));
            }
        }
        [JsonIgnore]
        public string CurrentSettingTitle
        {
            get
            {
                switch (_currentSettingPage)
                {
                    case PageType.Common: return "常规";
                    case PageType.Personalize: return "个性化";
                    case PageType.Data: return "数据";
                    case PageType.Network: return "网络";
                    default: return string.Empty;
                }
            }
        }

        static SettingsModel()
        {
            try
            {
                _settings = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(PathTools.SettingPath));
            }
            catch (Exception)
            {
            }
            if (_settings == null) _settings = new SettingsModel();
            if (_settings.Common.IsStartupEnabled)
            {
                SettingsCommon.SetStartup(true);
            }
            _settings.Common.PropertyChanged += OnChildPropertyChanged;
            _settings.Data.PropertyChanged += OnChildPropertyChanged;
            _settings.Network.PropertyChanged += OnChildPropertyChanged;
            _settings.Personalize.PropertyChanged += OnChildPropertyChanged;
        }

        private static void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _settings?.Save();
        }

        private SettingsModel()
        {
        }

        public static SettingsModel Load()
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

        public async Task SaveReal(CancellationToken? token = null)
        {
            try
            {
                if (token != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token.Value);
                }
                File.WriteAllText(PathTools.SettingPath, ToString());
                saveCts?.Cancel();
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
