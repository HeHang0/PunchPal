using PunchPal.Core.Models;
using PunchPal.Core.Services;
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
        public List<WorkingHours> ItemsAll { get; } = new List<WorkingHours>();

        public async Task InitItems(DateTime dateTime, DateTime start, DateTime end, IEnumerable<PunchRecord> punchRecords, IEnumerable<AttendanceRecord> attendanceRecords)
        {
            Items.Clear();
            ItemsAll.Clear();
            ItemsAll.AddRange(await GetRecords(start, end, punchRecords, attendanceRecords));
            ItemsAll.ForEach(m =>
            {
                if (m.WorkingDateTime.Month == dateTime.Month)
                {
                    Items.Add(m);
                }
            });
        }

        public async Task<List<WorkingHours>> GetRecords(DateTime start, DateTime end, IEnumerable<PunchRecord> punchRecords, IEnumerable<AttendanceRecord> attendanceRecords)
        {
            var settings = SettingsModel.Load();
            return await WorkHourService.Instance.List(settings.Data.DayStartHour, start, end, punchRecords, attendanceRecords);
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
