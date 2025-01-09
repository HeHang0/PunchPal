using PunchPal.WPF.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace PunchPal.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private MainModel _mainModel;
        public MainWindow()
        {
            InitializeComponent();
            _mainModel = new MainModel();
            DataContext = _mainModel;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            PunchNavigationView.Navigate("记录");
        }

        private void PunchNavigationView_Navigating(NavigationView sender, NavigatingCancelEventArgs args)
        {
            if (!(args.Page is Page page))
            {
                return;
            }
            if(page.DataContext != null)
            {
                return;
            }
            switch (page.GetType())
            {
                case var type when type == typeof(Pages.PunchRecordPage):
                    break;
                case var type when type == typeof(Pages.WorkingHoursPage):
                    break;
                case var type when type == typeof(Pages.CalendarPage):
                    page.DataContext = _mainModel.Calendar;
                    break;
                case var type when type == typeof(Pages.OverviewPage):
                    break;
                case var type when type == typeof(Pages.SettingsPage):
                    break;
            }
        }
    }
}