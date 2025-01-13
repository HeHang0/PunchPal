using PunchPal.Core.Events;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class PunchRecordVM : INotifyPropertyChanged
    {
        public event EventHandler<ConfirmDialogEventArgs> ConfirmDialog;

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task InitItems(DateTime dateTime)
        {
            Items.Clear();
            var settings = SettingsModel.Load();
            var dateStart = new DateTime(dateTime.Year, dateTime.Month, 1, settings.Data.DayStartHour, 0, 0);
            var dateEnd = dateStart.AddMonths(1);
            var dateStartValue = dateStart.TimestampUnix();
            var dateEndValue = dateEnd.TimestampUnix();
            var userId = settings.Common.CurrentUser?.UserId ?? "";
            var result = await PunchRecordService.Instance.List(m => m.UserId == userId && m.PunchTime >= dateStartValue && m.PunchTime < dateEndValue);
            foreach (var item in result)
            {
                Items.Add(item);
            }
        }

        public PunchRecord SelectedRecord { get; set; }

        public ICommand RemoveRecordCommand => new ActionCommand(OnRemoveRecord);

        private async void OnRemoveRecord()
        {
            if (SelectedRecord == null)
            {
                return;
            }
            var option = new ConfirmDialogEventArgs()
            {
                Title = "提示",
                Message = $"确认要删除【{SelectedRecord.PunchDateTimeText}】的记录吗?",
                Appearance = ControlAppearance.Danger
            };
            ConfirmDialog?.Invoke(this, option);
            if (option.Result != null && !(await option.Result))
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

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
