using Newtonsoft.Json;
using PunchPal.Tools;
using System.Collections.Generic;

namespace PunchPal.Core.ViewModels
{
    public class MonthSchedule : NotifyPropertyBase
    {
        private int _day = 1;
        public int Day
        {
            get => _day;
            set
            {
                _day = value;
                OnPropertyChanged();
            }
        }

        private string _remark = string.Empty;
        public string Remark
        {
            get => _remark;
            set
            {
                _remark = value;
                OnPropertyChanged();
            }
        }

        private bool _moveUpWhenWeekend = false;
        public bool MoveUpWhenWeekend
        {
            get => _moveUpWhenWeekend;
            set
            {
                _moveUpWhenWeekend = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore] public List<int> DayList => DateTimeTools.DayList;
    }
}
