﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PunchPal.WPF.Converters
{
    public class RectangleToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Rectangle rectangle)
            {
                return new Thickness(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }
            else
            {
                return new Thickness(0);
            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return new Rectangle((int)thickness.Left, (int)thickness.Top, (int)thickness.Right, (int)thickness.Bottom);
            }
            else
            {
                return new Rectangle();
            }
        }
    }
}
