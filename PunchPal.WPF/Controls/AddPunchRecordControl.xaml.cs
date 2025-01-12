using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace PunchPal.WPF.Controls
{
    /// <summary>
    /// AddPunchRecordControl.xaml 的交互逻辑
    /// </summary>
    public partial class AddPunchRecordControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AddPunchRecordControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        static AddPunchRecordControl()
        {
            for (var i = 0; i < 60; i++)
            {
                if (i < 24)
                {
                    HoursList.Add(i);
                }
                MinutesList.Add(i);
            }
        }

        public static List<int> HoursList { get; } = new List<int>();
        public static List<int> MinutesList { get; } = new List<int>();
        public static List<int> SecondsList => MinutesList;
        private DateTime _recordDate = DateTime.Now;
        public DateTime RecordDate
        {
            get => _recordDate;
            set
            {
                _recordDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RecordDateText));
            }
        }

        public DateTime RecordDateTime => new DateTime(_recordDate.Year, _recordDate.Month, _recordDate.Day, _hour, _minute, _second);
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

        public string RecordDateText => _recordDate.ToDateString();

        private int _second;
        public int Second
        {
            get => _second;
            set
            {
                _second = value;
                OnPropertyChanged();
            }
        }
        private int _minute;
        public int Minute
        {
            get => _minute;
            set
            {
                _minute = value;
                OnPropertyChanged();
            }
        }
        private int _hour;
        public int Hour
        {
            get => _hour;
            set
            {
                _hour = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
