using Newtonsoft.Json;
using PunchPal.Core.Models;
using PunchPal.Core.Services;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PunchPal.Core.ViewModels
{
    public class SettingsWorkingTimeRange : NotifyPropertyBase
    {
        public event EventHandler<WorkingTimeRange> Edited;

        [JsonIgnore]
        public ObservableCollection<WorkingTimeRange> Items { get; private set; } = new ObservableCollection<WorkingTimeRange>();

        [JsonIgnore]
        public WorkingTimeRange SelectedItem { get; set; }

        private int _flexibleWorkingMinute = 0;
        public int FlexibleWorkingMinute
        {
            get => _flexibleWorkingMinute;
            set
            {
                _flexibleWorkingMinute = value;
                OnPropertyChanged();
            }
        }

        private int _faultToleranceMinute = 0;
        public int FaultToleranceMinute
        {
            get => _faultToleranceMinute;
            set
            {
                _faultToleranceMinute = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public List<int> FaultToleranceMinuteList => DateTimeTools.MinutesList;

        private WorkingTimeRangeItems _currentItems = null;
        [JsonIgnore]
        public WorkingTimeRangeItems CurrentItems => _currentItems;
        public async Task InitRanges()
        {
            Items.Clear();
            var result = await WorkingTimeRangeService.Instance.List(m => true);
            foreach (var item in result)
            {
                Items.Add(item);
            }
            _currentItems = await WorkingTimeRangeService.Instance.CurrentItems();
            Text = _currentItems.Text;
        }

        private string _text = string.Empty;
        [JsonIgnore]
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public ICommand AddCommand => new ActionCommand(OnAdd);
        [JsonIgnore]
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
