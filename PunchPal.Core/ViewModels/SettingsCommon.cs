using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PunchPal.Core.ViewModels
{
    public class SettingsCommon
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isCalendarStartSun = true;
        public bool IsCalendarStartSun
        {
            get => _isCalendarStartSun;
            set
            {
                _isCalendarStartSun = value;
                OnPropertyChanged();
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
