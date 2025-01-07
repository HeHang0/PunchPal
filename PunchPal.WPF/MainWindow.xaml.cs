using PunchPal.WPF.ViewModels;
using Wpf.Ui.Controls;

namespace PunchPal.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainModel();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            PunchNavigationView.Navigate("记录");
        }
    }
}