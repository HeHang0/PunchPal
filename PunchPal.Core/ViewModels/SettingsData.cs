using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PunchPal.Core.ViewModels
{
    public class SettingsData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _everyDayWorkHour = 8;
        public int EveryDayWorkHour
        {
            get => _everyDayWorkHour;
            set
            {
                _everyDayWorkHour = value;
                OnPropertyChanged();
            }
        }

        private int _dayStartHour = 6;

        public int DayStartHour
        {
            get => _dayStartHour;
            set
            {
                if (value < 0 || value > 8)
                {
                    return;
                }

                _dayStartHour = value;
                OnPropertyChanged();
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
