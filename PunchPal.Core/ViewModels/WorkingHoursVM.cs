using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class WorkingHoursVM : NotifyPropertyBase
    {
        public event EventHandler<string> TextCoping;

        public async Task InitItems(DateTime dateTime, IEnumerable<CalendarRecord> calendars)
        {
            Items.Clear();
            var settings = SettingsModel.Load();
            var dateStart = new DateTime(dateTime.Year, dateTime.Month, 1, settings.Data.DayStartHour, 0, 0);
            var dateEnd = dateStart.AddMonths(1);
            var dateStartValue = dateStart.TimestampUnix();
            var dateEndValue = dateEnd.TimestampUnix();
            var userId = settings.Common.CurrentUser?.UserId ?? "";
            var result = await WorkHourService.Instance.List(settings.Data.DayStartHour, m => m.UserId == userId && m.PunchTime >= dateStartValue && m.PunchTime < dateEndValue, calendars);
            result.ForEach(m => Items.Add(m));
        }

        public async Task InitItems(IEnumerable<PunchRecord> punchRecords, IEnumerable<CalendarRecord> calendars)
        {
            Items.Clear();
            var settings = SettingsModel.Load();
            var result = await WorkHourService.Instance.List(settings.Data.DayStartHour, punchRecords, calendars);
            result.ForEach(m => Items.Add(m));
        }

        public ICommand CopyWorkHoursCommand => new ActionCommand(OnCopyWorkHours);

        private void OnCopyWorkHours()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("时间\t上班时间\t下班时间\t工时（分钟）\t \n");
            foreach (var item in Items)
            {
                stringBuilder.Append(item.ExportText);
                stringBuilder.Append("\n");
            }

            TextCoping?.Invoke(this, stringBuilder.ToString());
        }

        public ObservableCollection<WorkingHours> Items { get; } = new ObservableCollection<WorkingHours>();
    }
}
