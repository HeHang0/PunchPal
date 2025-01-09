using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PunchPal.WPF.Converters
{
    public class DrawBrushToMediaBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.SolidBrush drawBrush)
            {
                return new SolidColorBrush(Color.FromArgb(drawBrush.Color.A, drawBrush.Color.R, drawBrush.Color.G, drawBrush.Color.B));
            }
            else
            {
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B));
            }
            else
            {
                return System.Drawing.Brushes.Transparent;
            }
        }
    }
}
