using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PunchPal.WPF.Controls
{
    /// <summary>
    /// AddAttendanceRecordControl.xaml 的交互逻辑
    /// </summary>
    public partial class AddAttendanceRecordControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AddAttendanceRecordControl()
        {
            InitializeComponent();
            DataContext = this;
            InitTypes();
        }

        private async void InitTypes()
        {
            AttendanceTypes.Clear();
            var result = await AttendanceTypeService.Instance.List();
            foreach (var item in result)
            {
                AttendanceTypes.Add(item);
            }
            AttendanceTypeId = AttendanceTypes.FirstOrDefault()?.TypeId ?? string.Empty;
        }

        public static List<int> HoursList => DateTimeTools.HoursList;
        public static List<int> MinutesList => DateTimeTools.MinutesList;
        public static List<int> SecondsList => DateTimeTools.SecondsList;

        public ObservableCollection<AttendanceType> AttendanceTypes { get; } = new ObservableCollection<AttendanceType>();

        private string _attendanceTypeId = string.Empty;
        public string AttendanceTypeId
        {
            get => _attendanceTypeId;
            set
            {
                _attendanceTypeId = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDateTime => new DateTime(_startDate.Year, _startDate.Month, _startDate.Day, _startHour, _startMinute, _startSecond);

        private DateTime _startDate = DateTime.Now;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartDateText));
            }
        }

        private string _recordRemark = "";
        public string RecordRemark
        {
            get => _recordRemark;
            set
            {
                _recordRemark = value;
                OnPropertyChanged();
            }
        }

        public string StartDateText => _startDate.ToDateString();

        private int _startSecond;
        public int StartSecond
        {
            get => _startSecond;
            set
            {
                _startSecond = value;
                OnPropertyChanged();
            }
        }
        private int _startMinute;
        public int StartMinute
        {
            get => _startMinute;
            set
            {
                _startMinute = value;
                OnPropertyChanged();
            }
        }
        private int _startHour;
        public int StartHour
        {
            get => _startHour;
            set
            {
                _startHour = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDateTime => new DateTime(_endDate.Year, _endDate.Month, _endDate.Day, _endHour, _endMinute, _endSecond);

        private DateTime _endDate = DateTime.Now;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EndDateText));
            }
        }

        public string EndDateText => _endDate.ToDateString();

        private int _endSecond;
        public int EndSecond
        {
            get => _endSecond;
            set
            {
                _endSecond = value;
                OnPropertyChanged();
            }
        }
        private int _endMinute;
        public int EndMinute
        {
            get => _endMinute;
            set
            {
                _endMinute = value;
                OnPropertyChanged();
            }
        }
        private int _endHour;
        public int EndHour
        {
            get => _endHour;
            set
            {
                _endHour = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
