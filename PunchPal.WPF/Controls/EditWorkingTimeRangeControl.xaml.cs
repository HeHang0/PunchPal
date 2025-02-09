using PunchPal.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace PunchPal.WPF.Controls
{
    /// <summary>
    /// EditWorkingTimeRangeControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditWorkingTimeRangeControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public EditWorkingTimeRangeControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public List<KeyValuePair<WorkingTimeRangeType, string>> TypeList => WorkingTimeRange.TypeMappinng.ToList();

        public WorkingTimeRangeType _selectedType = WorkingTimeRangeType.Work;
        public WorkingTimeRangeType SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
            }
        }

        private DateTime _startTime = new DateTime().AddHours(9);
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _endTime = new DateTime().AddHours(17);
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _date = DateTime.Now.Date;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        private bool _isAllDate = true;
        public bool IsAllDate
        {
            get => _isAllDate;
            set
            {
                _isAllDate = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
