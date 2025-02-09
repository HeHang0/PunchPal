using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PunchPal.WPF.Converters
{
    class ActionToICommandConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Action action)
            {
                return new ActionCommand(action);
            }
            return null;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is ActionCommand command)
            {
                return new Action(() =>
                {
                    command.Execute(null);
                });
            }
            return null;
        }
    }
}
