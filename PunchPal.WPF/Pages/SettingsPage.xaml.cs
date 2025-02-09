using PunchPal.Core.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PunchPal.WPF.Pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsModel settingsModel &&
                !string.IsNullOrWhiteSpace(settingsModel.CurrentSettingTitle))
            {
                PunchNavigationView.Navigate(settingsModel.CurrentSettingTitle);
            }
            else
            {
                PunchNavigationView.Navigate("常规");
            }
        }
    }
}
