using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace PunchPal.WPF.Converters
{
    public class BooleanToCursorHandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hand = value is bool flag && flag;
            return hand ? Cursors.Hand : Cursors.Arrow;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is Cursor cursor ? cursor == Cursors.Hand : (object)true;
        }
    }
}
