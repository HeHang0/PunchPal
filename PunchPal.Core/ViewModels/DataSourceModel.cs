using Newtonsoft.Json;
using PunchPal.Core.Apis;
using PunchPal.Core.Events;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class DataSourceModel : NotifyPropertyBase
    {
        private static readonly DataSourceModel _dataSource;

        public ObservableCollection<DataSourceItem> Items { get; } = new ObservableCollection<DataSourceItem>();
        private DataSourceItem _selectedItem = null;
        public DataSourceItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand ImportDataSource => new ActionCommand(OnImportDataSource);
        public ICommand ExportDataSource => new ActionCommand(OnExportDataSource);
        public ICommand SaveDataSource => new ActionCommand(OnSaveDataSource);
        public ICommand TestRequest => new RelayCommand<DataSourceItem>(OnTestRequest);
        public ICommand AddDataSource => new RelayCommand<string>(OnAddDataSource);
        public ICommand RemoveItem => new RelayCommand<DataSourceItem>(OnRemoveItem);

        public void OnAddDataSource(string type)
        {
            if (!Enum.TryParse(type, out DataSourceItem.DataSourceType dataSourceType))
            {
                return;
            }
            if (dataSourceType == DataSourceItem.DataSourceType.UserInfo &&
                Items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.UserInfo) != null)
            {
                EventManager.ShowTips(new TipsOption("提示", "用户信息数据源只能添加一个"));
                return;
            }
            var item = new DataSourceItem(dataSourceType, DataSourceItem.RequestType.Post);
            item.ResetMappings();
            var index = -1;
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Type == dataSourceType)
                {
                    index = i;
                }
            }
            if (index >= 0 && index < Items.Count - 1)
            {
                Items.Insert(index + 1, item);
            }
            else
            {
                Items.Add(item);
            }
            SelectedItem = item;
        }

        public void OnRemoveItem(DataSourceItem item)
        {
            SelectedItem = null;
            Items.Remove(item);
        }

        public async void OnTestRequest(DataSourceItem item)
        {
            var headers = new Dictionary<string, string>
            {
                ["Cookie"] = ""
            };
            if (File.Exists(PathTools.CookiePath))
            {
                headers["Cookie"] = File.ReadAllText(PathTools.CookiePath);
            }
            var result = await item.RunRequest(GetPreData(DateTime.Now), headers, true);
            if (item.Type == DataSourceItem.DataSourceType.Authenticate && string.IsNullOrWhiteSpace(headers["Cookie"]))
            {
                headers["Cookie"] = result.Item2;
            }
            if (item.Type == DataSourceItem.DataSourceType.Authenticate && !string.IsNullOrWhiteSpace(headers["Cookie"]))
            {
                File.WriteAllText(PathTools.CookiePath, headers["Cookie"]);
            }
            if (item.Type == DataSourceItem.DataSourceType.Authenticate && item.RequestMethod == DataSourceItem.RequestType.Browser)
            {
                EventManager.ShowTips(new TipsOption("提示", $"{JsonConvert.SerializeObject(result.Item2)}"));
            }
            else if (result.Item1 != null)
            {
                if (result.Item1 is IEnumerable<dynamic> items)
                {
                    result.Item1 = items.Take(1);
                }
                EventManager.ShowTips(new TipsOption("提示", $"{JsonConvert.SerializeObject(result.Item1)}"));
            }
            else if (item.Type == DataSourceItem.DataSourceType.Authenticate)
            {
                EventManager.ShowTips(new TipsOption("提示", "请求失败"));
            }
            else
            {
                EventManager.ShowTips(new TipsOption("提示", "未获取到数据，请先执行一次认证信息"));
            }
        }

        private Dictionary<string, string> GetPreData(DateTime date)
        {
            var preData = new Dictionary<string, string>();
            if (File.Exists(PathTools.PreDataPath))
            {
                try
                {
                    preData = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PathTools.PreDataPath));
                }
                catch (Exception)
                {
                }
            }
            var start = new DateTime(date.Year, date.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            preData["YEAR"] = date.Year.ToString();
            preData["MONTH"] = date.Month.ToString();
            preData["DAYSTART"] = start.ToDateString();
            preData["DAYEND"] = end.ToDateString();
            preData["TIMESTART"] = start.ToDateTimeString();
            preData["TIMEEND"] = end.AddHours(23).AddMinutes(59).AddSeconds(59).ToDateTimeString();
            return preData;
        }

        public async Task<bool> SyncData(DateTime date)
        {
            var preData = GetPreData(date);
            var headers = new Dictionary<string, string>
            {
                ["Cookie"] = string.Empty
            };
            if (File.Exists(PathTools.CookiePath))
            {
                headers["Cookie"] = File.ReadAllText(PathTools.CookiePath);
            }
            User user = await CheckAuth(preData, headers);
            user = await UpdateUser(user);
            if (user == null)
            {
                return false;
            }
            SettingsModel.Load().Common.CurrentUser = user;
            if (!string.IsNullOrWhiteSpace(headers["Cookie"]))
            {
                File.WriteAllText(PathTools.CookiePath, headers["Cookie"]);
                File.WriteAllText(PathTools.PreDataPath, JsonConvert.SerializeObject(preData));
            }
            var browser = await PuppeteerBrowser.GetHeadlessBrowser();
            var ok = false;
            var punchTimeList = Items.Where(m => m.Type == DataSourceItem.DataSourceType.PunchTime);
            var attendanceList = Items.Where(m => m.Type == DataSourceItem.DataSourceType.Attendance);
            var calendarList = Items.Where(m => m.Type == DataSourceItem.DataSourceType.Calendar);
            var punchCount = 0;
            var attendanceCount = 0;
            var calendarCount = 0;
            foreach (var item in punchTimeList)
            {
                var (punch, _) = await item.RunRequest(preData, headers, browser: browser);
                if (punch is List<PunchRecord> punchRecords)
                {
                    punchRecords.ForEach(m => m.UserId = user.UserId);
                    await PunchRecordService.Instance.Add(punchRecords);
                    ok = true;
                    punchCount += punchRecords.Count;
                }
            }
            foreach (var item in attendanceList)
            {
                var (attendance, _) = await item.RunRequest(preData, headers, browser: browser);
                if (attendance is List<AttendanceRecord> attendanceRecords)
                {
                    attendanceRecords.ForEach(m => m.UserId = user.UserId);
                    await AttendanceRecordService.Instance.Add(attendanceRecords);
                    ok = true;
                    attendanceCount += attendanceRecords.Count;
                }
            }
            foreach (var item in calendarList)
            {
                var (calendar, _) = await item.RunRequest(preData, headers, browser: browser);
                if (calendar is List<CalendarRecord> calendarRecords)
                {
                    calendarRecords.ForEach(m => m.Type = CalendarType.DataSource);
                    await CalendarService.Instance.Add(calendarRecords);
                    ok = true;
                    calendarCount += calendarRecords.Count;
                }
            }
            try
            {
                await browser?.CloseAsync();
            }
            catch (Exception)
            {
                // ignore
            }
            if (ok)
            {
                var result = new List<string>();
                if (punchCount > 0)
                {
                    result.Add($"打卡记录：{punchCount}条");
                }
                if (attendanceCount > 0)
                {
                    result.Add($"考勤记录：{attendanceCount}条");
                }
                if (calendarCount > 0)
                {
                    result.Add($"日历记录：{calendarCount}条");
                }
                EventManager.ShowTips(new TipsOption("提示", $"数据同步成功！\n" + string.Join(" ", result)));
            }
            else
            {
                EventManager.ShowTips(new TipsOption("提示", "未同步到任何数据！"));
            }
            return ok;
        }

        private async Task<User> UpdateUser(User user)
        {
            var users = await UserService.Instance.List();
            if (user == null)
            {
                return users.FirstOrDefault();
            }
            if (users.Count == 1 && (users[0].Remark?.Contains("初始") ?? false))
            {
                await UserService.Instance.Add(user);
                if (!string.IsNullOrWhiteSpace(user.UserId))
                {
                    await UpdateTable(nameof(PunchRecord), nameof(PunchRecord.UserId), user.UserId);
                    await UpdateTable(nameof(AttendanceRecord), nameof(AttendanceRecord.UserId), user.UserId);
                    await UpdateTable(nameof(WorkingTimeRange), nameof(WorkingTimeRange.UserId), user.UserId);
                }
                await UserService.Instance.Remove(users[0]);
            }

            return user;
        }

        public async Task<bool> UpdateTable(string table, string key, string value)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    await context.Database.ExecuteSqlCommandAsync($"UPDATE {table} SET {key} = {value}");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<User> CheckAuth(Dictionary<string, string> preData, Dictionary<string, string> headers)
        {
            var UserInfo = Items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.UserInfo);
            var (user, _) = await UserInfo?.RunRequest(preData);
            if (user == null)
            {
                var authenticates = Items.Where(m => m.Type == DataSourceItem.DataSourceType.Authenticate);
                var cookies = new List<string>() { };
                if (!string.IsNullOrWhiteSpace(headers["Cookie"]))
                {
                    cookies.Add(headers["Cookie"]);
                }
                foreach (var authenticate in authenticates)
                {
                    var (auth, cookieAuth) = await authenticate.RunRequest(preData, headers);
                    if (auth is Dictionary<string, string> authMap)
                    {
                        foreach (var item in authMap)
                        {
                            preData[item.Key] = item.Value;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(cookieAuth))
                    {
                        cookies.Add(cookieAuth);
                        headers["Cookie"] = string.Join("; ", NetworkUtils.CookieToMap(string.Join("; ", cookies)).Select(item => $"{item.Key}={item.Value}"));
                    }
                }
                var (user1, _) = await UserInfo.RunRequest(preData, headers);
                user = user1;
            }
            return user as User;
        }

        private void OnImportDataSource()
        {
            var fileNames = EventManager.ShowFileDialog(new EventManager.FileDialogOption()
            {
                Filter = "数据源文件|*.json"
            });
            if (fileNames != null && fileNames.Length > 0)
            {
                SetDataSource(fileNames[0]);
                _ = SaveReal();
            }
        }

        private void OnExportDataSource()
        {
            var fileName = EventManager.ShowSaveDialog(new EventManager.SaveDialogOption()
            {
                Title = "保存文件",
                Filter = "数据源文件|*.json",
                DefaultExt = ".json",
                FileName = "DataSource.json",
                AddExtension = true
            });
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    File.WriteAllText(fileName, ToString());
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private void OnSaveDataSource()
        {
            _ = SaveReal();
        }

        static DataSourceModel()
        {
            _dataSource = new DataSourceModel();
            SetDataSource(PathTools.DataSourcePath, true);
        }

        private static void SetDataSource(string filePath, bool force = false)
        {
            List<DataSourceItem> items = new List<DataSourceItem>();
            try
            {
                items = JsonConvert.DeserializeObject<List<DataSourceItem>>(File.ReadAllText(filePath));
            }
            catch (Exception)
            {
            }
            if (items.Count > 0 || force)
            {
                _dataSource.ResetItems(items);
            }
        }

        private DataSourceModel()
        {
        }

        public void ResetItems(List<DataSourceItem> items)
        {
            Items.Clear();
            items.ForEach(m => Items.Add(m));
            foreach (var item in Items)
            {
                item.ResetMappings();
            }
        }

        public static DataSourceModel Load()
        {
            return _dataSource;
        }

        public override string ToString()
        {
            try
            {
                return JsonConvert.SerializeObject(Items);
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

        public async Task OnSave()
        {
            saveCts?.Cancel();
            await SaveReal();
        }

        public async Task SaveReal(CancellationToken? token = null)
        {
            try
            {
                if (token != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token.Value);
                }
                File.WriteAllText(PathTools.DataSourcePath, ToString());
                saveCts?.Cancel();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
