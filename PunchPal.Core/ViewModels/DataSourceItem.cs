using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PunchPal.Core.Apis;
using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
                    OnAddRequestMapping(nameof(CalendarRecord.IsCustomWeekend));
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
            private static readonly string[] CompareItemsOrigin = new string[]
            {
                "==", "!=", ">", "<", ">=", "<="
            };
            public string[] CompareItems => CompareItemsOrigin;
        }
        [JsonIgnore]
        private readonly HashSet<string> requestCacheSet = new HashSet<string>();
        private bool _loading = false;
        [JsonIgnore]
        public bool Loading
        {
            get => _loading;
            set
            {
                _loading = value;
                OnPropertyChanged();
            }
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
        public ObservableCollection<RequestMapping> BrowserMappings { get; set; } = new ObservableCollection<RequestMapping>() { new RequestMapping { Scripts = "关闭" } };

        [JsonIgnore]
        public ICommand AddRequestHeader => new ActionCommand(OnAddRequestHeader);
        [JsonIgnore]
        public ICommand AddRequestFilter => new ActionCommand(OnAddRequestFilter);
        [JsonIgnore]
        public ICommand AddRequestMapping => new ActionCommand(OnAddRequestMapping);
        [JsonIgnore]
        public ICommand AddBrowserMappings => new ActionCommand(OnAddBrowserMappings);
        [JsonIgnore]
        public ICommand RemoveFilter => new RelayCommand<RequestMapping>(OnRemoveFilter);
        [JsonIgnore]
        public ICommand RemoveHeader => new RelayCommand<RequestMapping>(OnRemoveHeader);
        [JsonIgnore]
        public ICommand RemoveMapping => new RelayCommand<RequestMapping>(OnRemoveMapping);

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
            RequestFilters.Add(new RequestMapping() { Scripts = "==" });
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
            BrowserMappings.Add(new RequestMapping { Scripts = "跳转" });
        }

        private void OnRemoveFilter(RequestMapping mapping)
        {
            RequestFilters.Remove(mapping);
        }

        private void OnRemoveHeader(RequestMapping mapping)
        {
            RequestHeaders.Remove(mapping);
        }

        private void OnRemoveMapping(RequestMapping mapping)
        {
            RequestMappings.Remove(mapping);
        }

        public async Task<(object, string)> RunRequest(Dictionary<string, string> preData = null, Dictionary<string, string> headers = null, bool test = false)
        {
            (object, string) result = (null, string.Empty);
            if (string.IsNullOrWhiteSpace(RequestUrl))
            {
                return result;
            }
            Loading = true;
            await Task.Run(async () =>
            {
                result = await RunRequestAsync(preData, headers, test);
            });
            Loading = false;
            return result;
        }

        public async Task<(object, string)> RunRequestAsync(Dictionary<string, string> preData = null, Dictionary<string, string> headers = null, bool test = false)
        {
            var url = ReplaceValue(RequestUrl, preData);
            if (RequestMethod == RequestType.Browser)
            {
                var navigation = new Dictionary<string, string>();
                var closeMappings = BrowserMappings.FirstOrDefault(m => m.Scripts == "关闭");
                var closeUrl = closeMappings?.Key ?? closeMappings?.Value ?? string.Empty;
                foreach (var item in BrowserMappings)
                {
                    if (item.Scripts == "跳转")
                    {
                        navigation[item.Key] = item.Value;
                    }
                }
                var result = await new PuppeteerBrowser().Run(url, closeUrl, navigation, true);
                if (result == null)
                {
                    result = await new PuppeteerBrowser().Run(url, closeUrl, navigation, false);
                }
                var cookies = new List<string>();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        cookies.Add($"{item.Key}={item.Value}");
                    }
                }
                return (null, string.Join("; ", cookies));
            }
            var body = ReplaceValue(RequestBody, preData);
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            foreach (var item in RequestHeaders)
            {
                headers[item.Key] = ReplaceValue(item.Value, preData);
            }
            var indexText = url + body + RequestMethod.ToString() + JsonConvert.SerializeObject(headers);
            if (!test && Type == DataSourceType.Calendar && requestCacheSet.Contains(FileTools.CalculateTextMD5(indexText)))
            {
                return (null, string.Empty);
            }
            var (text, cookie) = await NetworkUtils.Request(url, body, RequestMethod.ToString(), headers);
            try
            {
                var result = (await ParseJsonData(JsonTools.ParsePath(ResponseValue), text), cookie);
                if (!test && Type == DataSourceType.Calendar)
                {
                    requestCacheSet.Add(FileTools.CalculateTextMD5(indexText));
                }
                return result;
            }
            catch (Exception)
            {
                return (null, string.Empty);
            }
        }

        private async Task<object> ParseJsonData(List<object> list, string text)
        {
            var jToken = JToken.Parse(text);
            JObject jo = null;
            JArray ja = null;
            foreach (var item in list)
            {
                if (item?.GetType() == typeof(string))
                {
                    jToken = jToken[item.ToString()];
                }
                else if (item?.GetType() == typeof(int))
                {
                    jToken = jToken[(int)item];
                }
            }
            if (jToken is JObject)
            {
                jo = (JObject)jToken;
            }
            else if (jToken is JArray)
            {
                ja = (JArray)jToken;
                for (var i = 0; i < ja.Count; i++)
                {
                    if (!CheckJTokenFilter(ja[i]))
                    {
                        ja.RemoveAt(i);
                        i--;
                    }
                }
            }
            switch (Type)
            {
                case DataSourceType.Authenticate:
                    var auth = new Dictionary<string, string>();
                    foreach (var item in RequestMappings)
                    {
                        auth[item.Key] = await ParseJsonItem(jo, item, string.Empty);
                    }
                    return auth;
                case DataSourceType.UserInfo:
                    var user = new User();
                    user.Name = await ParseJsonItem(jo, RequestMappings.FirstOrDefault(m => m.Key == nameof(User.Name)), user.Name);
                    user.Avator = await ParseJsonItem(jo, RequestMappings.FirstOrDefault(m => m.Key == nameof(User.Avator)), user.Avator);
                    user.UserId = await ParseJsonItem(jo, RequestMappings.FirstOrDefault(m => m.Key == nameof(User.UserId)), user.UserId);
                    user.Remark = await ParseJsonItem(jo, RequestMappings.FirstOrDefault(m => m.Key == nameof(User.Remark)), user.Remark);
                    return user;
                case DataSourceType.PunchTime:
                    var punchList = new List<PunchRecord>();
                    for (var i = 0; i < ja.Count; i++)
                    {
                        long defaultTime = 0;
                        var result = await ParseJsonArray((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(PunchRecord.PunchTime)), defaultTime);
                        var punchType = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(PunchRecord.PunchType)), string.Empty);
                        var remark = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(PunchRecord.Remark)), string.Empty);
                        foreach (var item in result)
                        {
                            punchList.Add(new PunchRecord
                            {
                                PunchTime = item,
                                PunchType = punchType,
                                Remark = remark
                            });
                        }
                    }
                    return punchList;
                case DataSourceType.Attendance:
                    var attendanceList = new List<AttendanceRecord>();
                    for (var i = 0; i < ja.Count; i++)
                    {
                        var attendance = new AttendanceRecord();
                        attendance.AttendanceId = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(AttendanceRecord.AttendanceId)), attendance.AttendanceId);
                        attendance.AttendanceTypeId = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(AttendanceRecord.AttendanceTypeId)), attendance.AttendanceTypeId);
                        attendance.StartTime = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(AttendanceRecord.StartTime)), attendance.StartTime);
                        attendance.EndTime = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(AttendanceRecord.EndTime)), attendance.EndTime);
                        attendance.AttendanceTime = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(AttendanceRecord.AttendanceTime)),
                            attendance.StartTime > 0 ? attendance.StartTime.Unix2DateTime().Date.TimestampUnix() : attendance.EndTime.Unix2DateTime().Date.TimestampUnix());
                        attendance.Remark = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(AttendanceRecord.Remark)), attendance.Remark);
                        attendanceList.Add(attendance);
                    }
                    return attendanceList;
                case DataSourceType.Calendar:
                    var calendarList = new List<CalendarRecord>();
                    for (var i = 0; i < ja.Count; i++)
                    {
                        var calendar = new CalendarRecord();
                        calendar.Date = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.Date)), calendar.Date);
                        calendar.Festival = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.Festival)), calendar.Festival);
                        calendar.LunarMonth = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.LunarMonth)), calendar.LunarMonth);
                        calendar.LunarDate = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.LunarDate)), calendar.LunarDate);
                        calendar.LunarYear = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.LunarYear)), calendar.LunarYear);
                        calendar.SolarTerm = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.SolarTerm)), calendar.SolarTerm);
                        calendar.IsHoliday = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.IsHoliday)), calendar.IsHoliday);
                        calendar.IsWorkday = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.IsWorkday)), calendar.IsWorkday);
                        calendar.IsCustomWeekend = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.IsCustomWeekend)), calendar.IsCustomWeekend);
                        calendar.Remark = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(CalendarRecord.Remark)), calendar.Remark);
                        calendar.Type = CalendarType.DataSource;
                        calendarList.Add(calendar);
                    }
                    ResetWeekendCalendar(calendarList);
                    return calendarList;
                case DataSourceType.WorkTime:
                    var workTimeList = new List<WorkingTimeRange>();
                    for (int i = 0; i < ja.Count; i++)
                    {
                        var workTime = new WorkingTimeRange();
                        workTime.Date = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.Date)), workTime.Date);
                        workTime.Type = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.Type)), workTime.Type);
                        workTime.StartHour = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.StartHour)), workTime.StartHour);
                        workTime.StartMinute = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.StartMinute)), workTime.StartMinute);
                        workTime.EndHour = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.EndHour)), workTime.EndHour);
                        workTime.EndMinute = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.EndMinute)), workTime.EndMinute);
                        workTime.UserId = await ParseJsonItem((JObject)ja[i], RequestMappings.FirstOrDefault(m => m.Key == nameof(WorkingTimeRange.UserId)), workTime.UserId);
                        workTimeList.Add(workTime);
                    }
                    return workTimeList;
            }
            return null;
        }

        private bool CheckJTokenFilter(JToken jToken)
        {
            foreach (var item in RequestFilters)
            {
                if (!(jToken is JObject jObject))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    continue;
                }
                var value = jObject.GetValue(item.Key)?.ToString();
                if (item.Scripts == "==")
                {
                    if (item.Value != value) return false;
                    continue;
                }
                else if (item.Scripts == "!=")
                {
                    if (item.Value == value) return false;
                    continue;
                }
                int.TryParse(item.Value, out int localInt);
                int.TryParse(value, out int remoteInt);
                switch (item.Scripts)
                {
                    case "<":
                        if (localInt >= remoteInt) return false;
                        break;
                    case ">":
                        if (localInt <= remoteInt) return false;
                        break;
                    case ">=":
                        if (localInt < remoteInt) return false;
                        break;
                    case "<=":
                        if (localInt > remoteInt) return false;
                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        private void ResetWeekendCalendar(List<CalendarRecord> calendarList)
        {
            var weekends = calendarList.Where(m => m.IsCustomWeekend).ToList();
            if (weekends.Count <= 0)
            {
                return;
            }
            var weekendSet = new HashSet<long>(weekends.Select(m => m.Date));
            var dateSet = new HashSet<long>(calendarList.Select(m => m.Date));
            var minDate = weekends.Min(m => m.Date).Unix2DateTime();
            var maxDate = weekends.Max(m => m.Date).Unix2DateTime();
            for (; minDate <= maxDate; minDate = minDate.AddDays(1))
            {
                var day = minDate.DayOfWeek;
                if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
                {
                    continue;
                }
                var timestamp = minDate.TimestampUnix();
                if (dateSet.Contains(timestamp) || weekendSet.Contains(timestamp))
                {
                    continue;
                }
                calendarList.Add(new CalendarRecord
                {
                    Date = minDate.TimestampUnix(),
                    IsWorkday = true,
                    Type = CalendarType.DataSource
                });
            }
            for (var i = 0; i < calendarList.Count; i++)
            {
                if (calendarList[i].IsCustomWeekend)
                {
                    calendarList.RemoveAt(i);
                    i--;
                }
            }
            calendarList.Sort((x, y) => x.Date.CompareTo(y.Date));
        }

        private static readonly Assembly DateTimeAssembly = typeof(DateTime).Assembly;
        private static readonly ScriptOptions RoyalScriptOptions = ScriptOptions.Default
                        .AddReferences(DateTimeAssembly)
                        .WithImports("System");
        private static readonly string RoyalScript = "public static int TimestampUnix(this DateTime dateTime)\r\n        {\r\n            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;\r\n        }\n";

        private async Task<string> ParseJsonText(JObject data, RequestMapping mapping)
        {
            if (mapping == null)
            {
                return string.Empty;
            }
            var value = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(mapping.Scripts))
                {
                    var scripts = mapping.Scripts;
                    var valueMappings = mapping.Value?.Split(',') ?? new string[] { };
                    if (valueMappings.Length > 1)
                    {
                        for (var i = 0; i < valueMappings.Length; i++)
                        {
                            var valueItem = data[valueMappings[i].Trim()]?.ToString() ?? string.Empty;
                            scripts = scripts.Replace($"{{VALUE{i + 1}}}", valueItem);
                        }
                    }
                    else
                    {
                        scripts = scripts.Replace("{VALUE}", data[mapping.Value.Trim()]?.ToString() ?? string.Empty);
                    }
                    value = (await CSharpScript.EvaluateAsync<object>(
                        RoyalScript + scripts, RoyalScriptOptions)).ToString();
                }
                else
                {
                    value = data[mapping.Value]?.ToString() ?? string.Empty;
                }
            }
            catch (Exception)
            {
                //ignore
            }
            return value;
        }

        private T ParseJsonItem<T>(string value, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }
            var text = value;
            if (typeof(T) == typeof(string))
            {
                return (T)(object)text;
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)(text.ToLower() == "true");
            }
            else if (typeof(T) == typeof(long))
            {
                return (T)(object)long.Parse(text);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(text);
            }
            else if (typeof(T) == typeof(WorkingTimeRangeType))
            {
                return (T)(object)int.Parse(text);
            }
            return defaultValue;
        }

        private async Task<List<T>> ParseJsonArray<T>(JObject data, RequestMapping mapping, T defaultValue)
        {
            List<T> result = new List<T>();
            var value = await ParseJsonText(data, mapping);
            var values = value.Split(',');
            foreach (var item in values)
            {
                result.Add(ParseJsonItem(item, defaultValue));
            }
            return result;
        }

        private async Task<T> ParseJsonItem<T>(JObject data, RequestMapping mapping, T defaultValue)
        {
            var value = await ParseJsonText(data, mapping);
            return ParseJsonItem(value, defaultValue);
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
