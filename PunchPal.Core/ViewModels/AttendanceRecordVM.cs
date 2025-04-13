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
    public class AttendanceRecordVM : NotifyPropertyBase
    {
        public List<AttendanceRecord> ItemsAll { get; } = new List<AttendanceRecord>();
        public async Task InitItems(DateTime dateTime, DateTime start, DateTime end)
        {
            Items.Clear();
            ItemsAll.Clear();
            ItemsAll.AddRange(await GetRecords(start, end));
            foreach (var item in ItemsAll)
            {
                if (item.StartDateTime?.Month == dateTime.Month || item.EndDateTime?.Month == dateTime.Month)
                {
                    Items.Add(item);
                }
            }
        }
        public async Task<List<AttendanceRecord>> GetRecords(DateTime start, DateTime end)
        {
            Items.Clear();
            var settings = SettingsModel.Load();
            var dateStart = new DateTime(start.Year, start.Month, start.Day, settings.Data.DayStartHour, 0, 0);
            var dateEnd = new DateTime(end.Year, end.Month, end.Day, settings.Data.DayStartHour, 0, 0);
            var dateStartValue = dateStart.TimestampUnix();
            var dateEndValue = dateEnd.TimestampUnix();
            var userId = settings.Common.CurrentUser?.UserId ?? "";
            return await AttendanceRecordService.Instance.List(m => m.UserId == userId &&
                ((m.StartTime >= dateStartValue && m.StartTime < dateEndValue) ||
                (m.EndTime >= dateStartValue && m.EndTime < dateEndValue)));
        }

        public ObservableCollection<AttendanceRecord> Items { get; } = new ObservableCollection<AttendanceRecord>();

        public AttendanceRecord SelectedRecord { get; set; }

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
                Message = $"确认要删除【{SelectedRecord.AttendanceId}】的记录吗?",
                Appearance = ControlAppearance.Danger
            };
            var confirm = await EventManager.ShowConfirmDialog(option);
            if (!confirm)
            {
                return;
            }
            var result = await AttendanceRecordService.Instance.Remove(SelectedRecord);
            if (result)
            {
                Items.Remove(SelectedRecord);
                SelectedRecord = null;
            }
        }
    }
}
