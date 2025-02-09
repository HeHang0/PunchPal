using PunchPal.Core.Models;
using PunchPal.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

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

        private DateTime _startDateTime = DateTime.Now.Date;
        public DateTime StartDateTime
        {
            get => _startDateTime;
            set
            {
                _startDateTime = value;
                OnPropertyChanged();
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

        private DateTime _endDateTime = DateTime.Now.Date;
        public DateTime EndDateTime
        {
            get => _endDateTime;
            set
            {
                _endDateTime = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
