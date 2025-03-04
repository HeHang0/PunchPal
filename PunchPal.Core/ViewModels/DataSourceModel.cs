using Newtonsoft.Json;
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

        public DataSourceItem Authenticate { get; set; }
        public DataSourceItem UserInfo { get; set; }
        public DataSourceItem PunchTime { get; set; }
        public DataSourceItem Attendance { get; set; }
        public DataSourceItem Calendar { get; set; }
        public DataSourceItem WorkTime { get; set; }

        public ICommand ImportDataSource => new ActionCommand(OnImportDataSource);
        public ICommand ExportDataSource => new ActionCommand(OnExportDataSource);
        public ICommand SaveDataSource => new ActionCommand(OnSaveDataSource);
        public ICommand TestRequest => new RelayCommand<DataSourceItem>(OnTestRequest);

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
            var preData = new Dictionary<string, string>
            {
                ["YEAR"] = date.Year.ToString(),
                ["MONTH"] = date.Month.ToString()
            };
            var start = new DateTime(date.Year, date.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
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
                ["Cookie"] = ""
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
            }
            var ok = false;
            var (punch, _) = await PunchTime.RunRequest(preData, headers);
            if (punch is List<PunchRecord> punchRecords)
            {
                punchRecords.ForEach(m => m.UserId = user.UserId);
                await PunchRecordService.Instance.Add(punchRecords);
                ok = true;
            }
            var (attendance, _) = await Attendance.RunRequest(preData, headers);
            if (attendance is List<AttendanceRecord> attendanceRecords)
            {
                attendanceRecords.ForEach(m => m.UserId = user.UserId);
                await AttendanceRecordService.Instance.Add(attendanceRecords);
                ok = true;
            }
            var (calendar, _) = await Calendar.RunRequest(preData, headers);
            if (calendar is List<CalendarRecord> calendarRecords)
            {
                await CalendarService.Instance.Add(calendarRecords);
                ok = true;
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
            var (user, _) = await UserInfo.RunRequest(preData);
            if (user == null)
            {
                var (auth, cookieAuth) = await Authenticate.RunRequest(preData);
                if (auth == null && string.IsNullOrWhiteSpace(cookieAuth))
                {
                    return null;
                }
                headers["Cookie"] = cookieAuth;
                if (auth is Dictionary<string, string> authMap)
                {
                    foreach (var item in authMap)
                    {
                        preData[item.Key] = item.Value;
                    }
                }
                var (user1, _) = await UserInfo.RunRequest(preData);
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
                _dataSource.Authenticate = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.Authenticate) ?? new DataSourceItem(DataSourceItem.DataSourceType.Authenticate, DataSourceItem.RequestType.Get);
                _dataSource.UserInfo = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.UserInfo) ?? new DataSourceItem(DataSourceItem.DataSourceType.UserInfo, DataSourceItem.RequestType.Get);
                _dataSource.PunchTime = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.PunchTime) ?? new DataSourceItem(DataSourceItem.DataSourceType.PunchTime);
                _dataSource.Attendance = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.Attendance) ?? new DataSourceItem(DataSourceItem.DataSourceType.Attendance);
                _dataSource.Calendar = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.Calendar) ?? new DataSourceItem(DataSourceItem.DataSourceType.Calendar);
                _dataSource.WorkTime = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.WorkTime) ?? new DataSourceItem(DataSourceItem.DataSourceType.WorkTime);
                _dataSource.ResetItems();
            }
        }

        private DataSourceModel()
        {
        }

        public void ResetItems()
        {
            Items.Clear();
            Items.Add(Authenticate);
            Items.Add(UserInfo);
            Items.Add(PunchTime);
            Items.Add(Attendance);
            Items.Add(Calendar);
            //Items.Add(WorkTime);
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
