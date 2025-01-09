using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PunchPal.WPF.Converters
{
    public class BooleanToUnderlineDecorationsConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inUnderline = value is bool flag && flag;
            return inUnderline ? TextDecorations.Underline : null;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is TextDecorationCollection decorations ? decorations == TextDecorations.Underline : (object)true;
        }
    }
}
