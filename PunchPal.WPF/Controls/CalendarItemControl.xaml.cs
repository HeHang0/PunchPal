using System.Windows.Controls;

namespace PunchPal.WPF.Controls
{
    /// <summary>
    /// CalendarItem.xaml 的交互逻辑
    /// </summary>
    public partial class CalendarItemControl : UserControl
    {
        public CalendarItemControl()
        {
            InitializeComponent();
            Loaded += CalendarItemControl_Loaded;
        }

        private void CalendarItemControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
