using PunchPal.Core.Models;
using PunchPal.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsWorkingTimeRange : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<WorkingTimeRange> Edited;

        public ObservableCollection<WorkingTimeRange> Items { get; private set; } = new ObservableCollection<WorkingTimeRange>();

        public WorkingTimeRange SelectedItem { get; set; }

        public async Task InitRanges()
        {
            Items.Clear();
            var result = await WorkingTimeRangeService.Instance.List(m => true);
            foreach (var item in result)
            {
                Items.Add(item);
            }
        }

        public ICommand AddCommand => new ActionCommand(OnAdd);

        public ICommand EditCommand => new ActionCommand(OnEdit);

        private void OnEdit()
        {
            Edited?.Invoke(this, SelectedItem);
        }

        private void OnAdd()
        {
            Edited?.Invoke(this, null);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
