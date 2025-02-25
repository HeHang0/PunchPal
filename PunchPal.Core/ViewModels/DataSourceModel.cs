﻿using Newtonsoft.Json;
using PunchPal.Core.Events;
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
            if (string.IsNullOrWhiteSpace(fileName))
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
                items = JsonConvert.DeserializeObject<List<DataSourceItem>>(File.ReadAllText(PathTools.DataSourcePath));
            }
            catch (Exception)
            {
            }
            if (items.Count > 0 || force)
            {
                _dataSource.Authenticate = items.FirstOrDefault(m => m.Type == DataSourceItem.DataSourceType.Authenticate) ?? new DataSourceItem(DataSourceItem.DataSourceType.Authenticate, DataSourceItem.RequestType.Browser);
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
            Items.Add(WorkTime);
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
