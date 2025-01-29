using PicaPico;
using PunchPal.Core.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PunchPal.WPF.Pages
{
    /// <summary>
    /// OverviewPage.xaml 的交互逻辑
    /// </summary>
    public partial class OverviewPage : Page
    {
        public OverviewPage()
        {
            InitializeComponent();
        }

        private void Page_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if(!(DataContext is OverviewVM overview))
            {
                return;
            }
            overview.PropertyChanged += Overview_PropertyChanged;
        }

        private void Overview_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(OverviewVM.ChartSeries) ||
                e.PropertyName == nameof(OverviewVM.IsDarkMode))
            {
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            Task.Delay(TimeSpan.FromMilliseconds(100)).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    HourPirChart.CoreChart.Update();
                });
            });
        }
    }
}
