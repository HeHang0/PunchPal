using Newtonsoft.Json;
using PunchPal.Tools;
using ShellLink.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        static DataSourceModel()
        {
            List<DataSourceItem> items = new List<DataSourceItem>();
            try
            {
                items = JsonConvert.DeserializeObject<List<DataSourceItem>>(File.ReadAllText(PathTools.DataSourcePath));
            }
            catch (Exception)
            {
            }
            _dataSource = new DataSourceModel();
            _dataSource.Authenticate = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.Authenticate) ?? new DataSourceItem(DataSourceItem.DataSourceType.Authenticate, DataSourceRequestType.Browser);
            _dataSource.UserInfo = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.UserInfo) ?? new DataSourceItem(DataSourceItem.DataSourceType.UserInfo, DataSourceRequestType.Get);
            _dataSource.PunchTime = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.PunchTime) ?? new DataSourceItem(DataSourceItem.DataSourceType.PunchTime);
            _dataSource.Attendance = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.Attendance) ?? new DataSourceItem(DataSourceItem.DataSourceType.Attendance);
            _dataSource.ResetItems();
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
