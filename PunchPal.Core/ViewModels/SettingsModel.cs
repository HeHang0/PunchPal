using Newtonsoft.Json;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsModel : NotifyPropertyBase
    {
        private static readonly SettingsModel _settings;

        public SettingsData Data { get; set; } = new SettingsData();
        public SettingsCalendar Calendar { get; set; } = new SettingsCalendar();
        public SettingsCommon Common { get; set; } = new SettingsCommon();
        public SettingsNetwork Network { get; set; } = new SettingsNetwork();
        public SettingsPersonalize Personalize { get; set; } = new SettingsPersonalize();
        [JsonIgnore] public SettingsAbout About { get; set; } = new SettingsAbout();
        [JsonIgnore] public DataSourceModel DataSource { get; set; } = DataSourceModel.Load();
        [JsonIgnore] public SettingsWorkingTimeRange WorkingTimeRange { get; set; } = new SettingsWorkingTimeRange();

        public enum PageType
        {
            Data,
            Common,
            Network,
            Personalize,
            Calendar,
            WorkingTimeRange,
            DataSource,
            Abount
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
                OnPropertyChanged(nameof(DataSourceOperateVisible));
                if (_currentSettingPage == PageType.WorkingTimeRange)
                {
                    _ = WorkingTimeRange.InitRanges();
                }
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
                    case PageType.DataSource: return "数据源";
                    case PageType.Network: return "网络";
                    case PageType.Calendar: return "日历";
                    case PageType.WorkingTimeRange: return "工作时间";
                    case PageType.Abount: return "关于";
                    default: return string.Empty;
                }
            }
        }
        [JsonIgnore]
        public bool DataSourceOperateVisible => _currentSettingPage == PageType.DataSource;

        private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                _isPaneOpen = value;
                OnPropertyChanged();
                Save();
            }
        }
        [JsonIgnore]
        public ICommand ImportDataSource => DataSource.ImportDataSource;
        [JsonIgnore]
        public ICommand ExportDataSource => DataSource.ExportDataSource;
        [JsonIgnore]
        public ICommand SaveDataSource => DataSource.SaveDataSource;

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
            _settings.Calendar.PropertyChanged += OnChildPropertyChanged;
            _settings.Data.PropertyChanged += OnChildPropertyChanged;
            _settings.Network.PropertyChanged += OnChildPropertyChanged;
            _settings.Personalize.PropertyChanged += OnChildPropertyChanged;
            if (_settings.Common.CurrentUser != null)
            {
                _settings.Common.CurrentUser = UserService.Instance.FirstOrDefault(m => m.UserId == _settings.Common.CurrentUser.UserId) ?? UserService.Instance.FirstOrDefault();
            }
            _ = _settings.WorkingTimeRange.InitRanges();
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
                await DataSource.SaveReal();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
