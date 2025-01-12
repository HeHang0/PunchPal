using PunchPal.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PunchPal.Core.ViewModels
{
    public class AttendanceRecordVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public async Task InitItems(DateTime dateTime)
        {
            Items.Clear();
        }

        public ObservableCollection<AttendanceRecord> Items { get; } = new ObservableCollection<AttendanceRecord>();

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
