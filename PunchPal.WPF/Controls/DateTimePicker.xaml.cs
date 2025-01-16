using PunchPal.Tools;
using System;
using System.Collections.Generic;
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
    /// DateTimePicker.xaml 的交互逻辑
    /// </summary>
    public partial class DateTimePicker : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum ModeType
        {
            DateTime,
            Date,
            Time
        }

        public DateTimePicker()
        {
            InitializeComponent();
            ControlGrid.DataContext = this;
            var dateTime = DateTime;
            _date = dateTime.Date;
            _hour = dateTime.Hour;
            _minute = dateTime.Minute;
            _second = dateTime.Second;
        }

        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register(
                nameof(DateTime),
                typeof(DateTime),
                typeof(DateTimePicker),
                new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, OnDateTimeChanged)
            );

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(ModeType),
                typeof(DateTimePicker),
                new FrameworkPropertyMetadata(ModeType.DateTime)
            );

        private static object OnDateTimeChanged(DependencyObject d, object baseValue)
        {
            if(d is DateTimePicker picker && baseValue is DateTime dateTime)
            {
                picker._isUpdating = true;
                picker.Date = dateTime.Date;
                picker.Hour = dateTime.Hour;
                picker.Minute = dateTime.Minute;
                picker.Second = dateTime.Second;
                picker._isUpdating = false;
            }
            return baseValue;
        }

        private bool _isUpdating = false;

        public static List<int> HoursList => DateTimeTools.HoursList;
        public static List<int> MinutesList => DateTimeTools.MinutesList;
        public static List<int> SecondsList => DateTimeTools.SecondsList;

        //public DateTime DateTime => new DateTime(_date.Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second);

        [Bindable(true)]
        public DateTime DateTime
        {
            get => (DateTime)GetValue(DateTimeProperty);
            set
            {
                SetValue(DateTimeProperty, new DateTime(_date.Year, _date.Month, _date.Day, Hour, Minute, Second));
            }
        }

        public ModeType Mode
        {
            get => (ModeType)GetValue(ModeProperty);
            set
            {
                SetValue(ModeProperty, value);
                OnPropertyChanged(nameof(IsDateMode));
                OnPropertyChanged(nameof(IsTimeMode));
                OnPropertyChanged(nameof(IsDateTimeMode));
            }
        }

        public bool IsDateMode => Mode == ModeType.Date || Mode == ModeType.DateTime;
        public bool IsTimeMode => Mode == ModeType.Time || Mode == ModeType.DateTime;
        public bool IsDateTimeMode => Mode == ModeType.DateTime;

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateText));
                OnDateTimePropertyChanged();
            }
        }

        public bool NotifyUpdated { get; set; } = true;

        public string DateText => Date.ToDateString();

        private int _second;
        public int Second
        {
            get => _second;
            set
            {
                _second = value;
                OnPropertyChanged();
                OnDateTimePropertyChanged();
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
                OnDateTimePropertyChanged();
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
                OnDateTimePropertyChanged();
            }
        }

        private void OnDateTimePropertyChanged()
        {
            if(_isUpdating)
            {
                return;
            }
            DateTime = new DateTime(Date.Year, Date.Month, Date.Day, Hour, Minute, Second);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
