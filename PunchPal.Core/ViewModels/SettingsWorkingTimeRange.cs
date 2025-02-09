using PunchPal.Core.Models;
using PunchPal.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsWorkingTimeRange : NotifyPropertyBase
    {
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
            var currentItems = await WorkingTimeRangeService.Instance.CurrentItems();
            Text = currentItems.Text;
        }

        public string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
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
    }
}
