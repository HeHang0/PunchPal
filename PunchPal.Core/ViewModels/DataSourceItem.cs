using Newtonsoft.Json;
using PunchPal.Core.Apis;
using PunchPal.Core.Events;
using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class DataSourceItem : NotifyPropertyBase
    {
        public enum RequestType
        {
            Get, Post, Put, Browser
        }
        public enum DataSourceType
        {
            Authenticate, UserInfo, PunchTime, Attendance, Calendar, WorkTime
        }

        public DataSourceItem(DataSourceType dataSourceType, RequestType requestType = RequestType.Post)
        {
            Type = dataSourceType;
            RequestMethod = requestType;
        }

        public void ResetMappings()
        {
            for (int i = 0; i < RequestMappings.Count;)
            {
                if (string.IsNullOrWhiteSpace(RequestMappings[i].Key))
                {
                    RequestMappings.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            switch (Type)
            {
                case DataSourceType.UserInfo:
                    OnAddRequestMapping(nameof(User.Name));
                    OnAddRequestMapping(nameof(User.Avator));
                    OnAddRequestMapping(nameof(User.UserId));
                    break;
                case DataSourceType.PunchTime:
                    OnAddRequestMapping(nameof(PunchRecord.PunchTime));
                    OnAddRequestMapping(nameof(PunchRecord.PunchType));
                    OnAddRequestMapping(nameof(PunchRecord.Remark));
                    break;
                case DataSourceType.Attendance:
                    OnAddRequestMapping(nameof(AttendanceRecord.AttendanceId));
                    OnAddRequestMapping(nameof(AttendanceRecord.AttendanceTypeId));
                    OnAddRequestMapping(nameof(AttendanceRecord.StartTime));
                    OnAddRequestMapping(nameof(AttendanceRecord.EndTime));
                    OnAddRequestMapping(nameof(AttendanceRecord.AttendanceTime));
                    OnAddRequestMapping(nameof(AttendanceRecord.Remark));
                    break;
                case DataSourceType.Calendar:
                    OnAddRequestMapping(nameof(CalendarRecord.Date));
                    OnAddRequestMapping(nameof(CalendarRecord.Festival));
                    OnAddRequestMapping(nameof(CalendarRecord.LunarMonth));
                    OnAddRequestMapping(nameof(CalendarRecord.LunarDate));
                    OnAddRequestMapping(nameof(CalendarRecord.LunarYear));
                    OnAddRequestMapping(nameof(CalendarRecord.SolarTerm));
                    OnAddRequestMapping(nameof(CalendarRecord.IsHoliday));
                    OnAddRequestMapping(nameof(CalendarRecord.IsWorkday));
                    OnAddRequestMapping(nameof(CalendarRecord.Remark));
                    break;
                case DataSourceType.WorkTime:
                    OnAddRequestMapping(nameof(WorkingTimeRange.Date));
                    OnAddRequestMapping(nameof(WorkingTimeRange.Type));
                    OnAddRequestMapping(nameof(WorkingTimeRange.StartHour));
                    OnAddRequestMapping(nameof(WorkingTimeRange.StartMinute));
                    OnAddRequestMapping(nameof(WorkingTimeRange.EndHour));
                    OnAddRequestMapping(nameof(WorkingTimeRange.EndMinute));
                    OnAddRequestMapping(nameof(WorkingTimeRange.UserId));
                    break;
            }
        }

        public class RequestMapping : NotifyPropertyBase
        {
            private string _key = string.Empty;
            public string Key
            {
                get => _key;
                set
                {
                    _key = value;
                    OnPropertyChanged();
                }
            }
            private string _value = string.Empty;
            public string Value
            {
                get => _value;
                set
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
            private string _scripts = string.Empty;
            public string Scripts
            {
                get => _scripts;
                set
                {
                    _scripts = value;
                    OnPropertyChanged();
                }
            }
            public bool IsReadOnly { get; set; } = true;
        }
        [JsonIgnore]
        public string Name
        {
            get
            {
                switch (Type)
                {
                    case DataSourceType.Authenticate:
                        return "认证";
                    case DataSourceType.UserInfo:
                        return "用户信息";
                    case DataSourceType.PunchTime:
                        return "打卡时间";
                    case DataSourceType.Attendance:
                        return "考勤";
                    case DataSourceType.Calendar:
                        return "日历";
                    case DataSourceType.WorkTime:
                        return "工作时间";
                    default:
                        return string.Empty;
                }
            }
        }
        public DataSourceType Type;
        private string _requestUrl = string.Empty;
        public string RequestUrl
        {
            get => _requestUrl;
            set
            {
                _requestUrl = value;
                OnPropertyChanged();
            }
        }
        private bool _isExpanded = true;
        [JsonIgnore]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
        private RequestType _requestMethod = RequestType.Get;
        public RequestType RequestMethod
        {
            get => _requestMethod;
            set
            {
                _requestMethod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBrowser));
            }
        }
        private string _requestBody = string.Empty;
        public string RequestBody
        {
            get => _requestBody;
            set
            {
                _requestBody = value;
                OnPropertyChanged();
            }
        }
        private string _responseValue = string.Empty;
        public string ResponseValue
        {
            get => _responseValue;
            set
            {
                _responseValue = value;
                OnPropertyChanged();
            }
        }

        private static readonly List<KeyValuePair<RequestType, string>> RequestTypesOrigin = new List<KeyValuePair<RequestType, string>>
        {
            new KeyValuePair<RequestType, string>(RequestType.Get, "Get"),
            new KeyValuePair<RequestType, string>(RequestType.Post, "Post"),
            new KeyValuePair<RequestType, string>(RequestType.Put, "Put"),
            new KeyValuePair<RequestType, string>(RequestType.Browser, "Browser")
        };

        [JsonIgnore]
        public List<KeyValuePair<RequestType, string>> RequestTypes => RequestTypesOrigin;

        public bool IsBrowser => RequestMethod == RequestType.Browser;
        public bool CanAddMapping => Type == DataSourceType.Authenticate;
        public bool RequestMappingVisible => CanAddMapping || RequestMappings.Any();
        public ObservableCollection<RequestMapping> RequestHeaders { get; set; } = new ObservableCollection<RequestMapping>();
        public ObservableCollection<RequestMapping> RequestFilters { get; set; } = new ObservableCollection<RequestMapping>();
        public ObservableCollection<RequestMapping> RequestMappings { get; set; } = new ObservableCollection<RequestMapping>();
        public ObservableCollection<RequestMapping> BrowserMappings { get; set; } = new ObservableCollection<RequestMapping>() { new RequestMapping { Scripts="关闭"  } };

        [JsonIgnore]
        public ICommand AddRequestHeader => new ActionCommand(OnAddRequestHeader);
        [JsonIgnore]
        public ICommand AddRequestFilter => new ActionCommand(OnAddRequestFilter);
        [JsonIgnore]
        public ICommand AddRequestMapping => new ActionCommand(OnAddRequestMapping);
        [JsonIgnore]
        public ICommand AddBrowserMappings => new ActionCommand(OnAddBrowserMappings);
        [JsonIgnore]
        public ICommand TestRequest => new ActionCommand(OnTestRequest);

        private void OnAddRequestHeader()
        {
            if (RequestHeaders.Any(m => string.IsNullOrEmpty(m.Key) && string.IsNullOrEmpty(m.Value)))
            {
                return;
            }
            RequestHeaders.Add(new RequestMapping());
        }

        private void OnAddRequestFilter()
        {
            if (RequestFilters.Any(m => string.IsNullOrEmpty(m.Key) && string.IsNullOrEmpty(m.Value)))
            {
                return;
            }
            RequestFilters.Add(new RequestMapping());
        }

        public void OnAddRequestMapping()
        {
            OnAddRequestMapping(string.Empty);
        }

        public void OnAddRequestMapping(string name)
        {
            if (RequestMappings.Any(m => m.Key == name))
            {
                return;
            }
            RequestMappings.Add(new RequestMapping { Key = name, IsReadOnly = false });
        }

        public void OnAddBrowserMappings()
        {
            if (BrowserMappings.Any(m => string.IsNullOrEmpty(m.Key) && string.IsNullOrEmpty(m.Value) && m.Scripts == "跳转"))
            {
                return;
            }
            BrowserMappings.Add(new RequestMapping { Scripts="跳转"  });
        }

        public void OnTestRequest()
        {
            var now = DateTime.Now;
            var preData = new Dictionary<string, string>();
            preData["YEAR"] = now.Year.ToString();
            preData["MONTH"] = now.Month.ToString();
            var start = new DateTime(now.Year, now.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            preData["DAYSTART"] = start.ToDateString();
            preData["DAYEND"] = end.ToDateString();
            _ = RunRequest(preData);
        }

        public async Task RunRequest(Dictionary<string, string> preData = null, Dictionary<string, string> headers = null)
        {
            var url = ReplaceValue(RequestUrl, preData);
            if (RequestMethod == RequestType.Browser)
            {
                var navigations = new Dictionary<string, string>();
                var closeMappings = BrowserMappings.FirstOrDefault(m => m.Scripts == "关闭");
                var closeUrl = closeMappings?.Key ?? closeMappings?.Value ?? string.Empty;
                foreach (var item in BrowserMappings)
                {
                    if (item.Scripts == "跳转")
                    {
                        navigations[item.Key] = item.Value;
                    }
                }
                var result = await new PuppeteerBrowser().Run(url, closeUrl, navigations, false);
                EventManager.ShowTips(new TipsOption("提示", $"{JsonConvert.SerializeObject(result)}"));
                return;
            }
            var body = ReplaceValue(RequestBody, preData);
            if(headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            foreach (var item in RequestHeaders)
            {
                headers[item.Key] = ReplaceValue(item.Value, preData);
            }
            EventManager.ShowTips(new TipsOption("提示", $"{url}\n{body}\n{JsonConvert.SerializeObject(headers)}"));
        }

        private string ReplaceValue(string text, Dictionary<string, string> preData)
        {
            foreach (var item in preData)
            {
                text = text.Replace($"{{{item.Key}}}", item.Value);
            }
            return text;
        }
    }
}
