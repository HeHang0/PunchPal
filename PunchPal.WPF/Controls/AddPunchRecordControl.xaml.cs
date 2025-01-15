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

        private DateTime _recordDateTime = DateTime.Now;
        public DateTime RecordDateTime
        {
            get => _recordDateTime;
            set
            {
                _recordDateTime = value;
                OnPropertyChanged();
            }
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
