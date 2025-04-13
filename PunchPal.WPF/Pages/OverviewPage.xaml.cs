using PunchPal.Core.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PunchPal.WPF.Pages
{
    /// <summary>
    /// OverviewPage.xaml 的交互逻辑
    /// </summary>
    public partial class OverviewPage : Page
    {
        CancellationTokenSource updateTokenSource = null;
        public OverviewPage()
        {
            InitializeComponent();
        }

        private void Page_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is OverviewVM oldOverview)
            {
                oldOverview.PropertyChanged -= Overview_PropertyChanged;
            }
            else if (e.NewValue is OverviewVM newOverview)
            {
                newOverview.PropertyChanged += Overview_PropertyChanged;
            }
        }

        private void Overview_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OverviewVM.ChartSeries) ||
                e.PropertyName == nameof(OverviewVM.IsDarkMode) ||
                e.PropertyName == nameof(OverviewVM.ChartWeekStackSeries) ||
                e.PropertyName == nameof(OverviewVM.ChartWeekStackXAxis) ||
                e.PropertyName == nameof(OverviewVM.IsWeeklySelected))
            {
                updateTokenSource?.Cancel();
                updateTokenSource = new CancellationTokenSource();
                UpdateChart();
            }
        }

        private async void UpdateChart()
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(233), updateTokenSource.Token);
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        HourPirChart.CoreChart.Update();
                        HourStackChart.CoreChart.Update();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }
    }
}
