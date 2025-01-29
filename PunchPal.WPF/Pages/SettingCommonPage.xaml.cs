using PunchPal.Core.ViewModels;
using System.Windows.Controls;

namespace PunchPal.WPF.Pages
{
    /// <summary>
    /// SettingCommonPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingCommonPage : Page
    {
        public SettingCommonPage()
        {
            InitializeComponent();
        }

        private void ShortcutTextBox_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(!(DataContext is SettingsCommon model))
            {
                return;
            }
            model.OnShortcutChanged((Core.Models.ModifierKeys)e.KeyboardDevice.Modifiers, (Core.Models.Key)e.Key);
        }
    }
}
