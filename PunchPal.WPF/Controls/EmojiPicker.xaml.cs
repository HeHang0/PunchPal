using PunchPal.WPF.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PunchPal.WPF.Controls
{
    /// <summary>
    /// EmojiPicker.xaml 的交互逻辑
    /// </summary>
    public partial class EmojiPicker : UserControl
    {
        Window _parentWindow = null;
        public EmojiPicker()
        {
            InitializeComponent();
            Loaded += EmojiPicker_Loaded;
        }

        private void EmojiPicker_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
        }

        // 定义可绑定的 Emoji 属性
        public static readonly DependencyProperty EmojiProperty =
            DependencyProperty.Register("Emoji", typeof(string), typeof(EmojiPicker),
                new FrameworkPropertyMetadata("😄", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Emoji
        {
            get => (string)GetValue(EmojiProperty);
            set => SetValue(EmojiProperty, value);
        }

        private void EmojiLabel_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopupService.ShowAt(this, selectedEmoji =>
            {
                Emoji = selectedEmoji;
            });
        }
    }
}
