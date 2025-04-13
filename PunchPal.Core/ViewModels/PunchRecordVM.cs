using PunchPal.Core.Events;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class PunchRecordVM : NotifyPropertyBase
    {
        public List<PunchRecord> ItemsAll { get; } = new List<PunchRecord>();
        public async Task InitItems(DateTime dateTime, DateTime start, DateTime end)
        {
            Items.Clear();
            ItemsAll.Clear();
            ItemsAll.AddRange(await GetRecords(start, end));
            foreach (var item in ItemsAll)
            {
                if (item.PunchDateTime.Month == dateTime.Month)
                {
                    Items.Add(item);
                }
            }
        }

        public async Task<List<PunchRecord>> GetRecords(DateTime start, DateTime end)
        {
            var settings = SettingsModel.Load();
            var dateStartValue = start.TimestampUnix();
            var dateEndValue = end.TimestampUnix();
            var userId = settings.Common.CurrentUser?.UserId ?? "";
            return await PunchRecordService.Instance.List(m => m.UserId == userId && m.PunchTime >= dateStartValue && m.PunchTime < dateEndValue);
        }

        public PunchRecord SelectedRecord { get; set; }

        public ICommand RemoveRecordCommand => new ActionCommand(OnRemoveRecord);

        private async void OnRemoveRecord()
        {
            if (SelectedRecord == null)
            {
                return;
            }
            var option = new EventManager.ConfirmDialogOption()
            {
                Title = "提示",
                Message = $"确认要删除【{SelectedRecord.PunchDateTimeText}】的记录吗?",
                Appearance = ControlAppearance.Danger
            };
            var confirm = await EventManager.ShowConfirmDialog(option);
            if (!confirm)
            {
                return;
            }
            var result = await PunchRecordService.Instance.Remove(SelectedRecord);
            if (result)
            {
                Items.Remove(SelectedRecord);
                SelectedRecord = null;
            }
        }

        public ObservableCollection<PunchRecord> Items { get; } = new ObservableCollection<PunchRecord>();
    }
}
