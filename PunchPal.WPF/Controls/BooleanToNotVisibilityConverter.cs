using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PunchPal.WPF.Controls
{
    public class BooleanToNotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inVisible = value is bool flag && flag;
            return inVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is Visibility visibility ? (object)(visibility == Visibility.Collapsed) : (object)true;
        }
    }
}
