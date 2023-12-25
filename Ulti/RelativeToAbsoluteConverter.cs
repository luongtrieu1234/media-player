using System;
using System.Globalization;
using System.Windows.Data;

namespace Ulti
{
    public class RelativeToAbsoluteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relative = (string)value;
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            string absolutePath = $"{folder}{relative}";

            return absolutePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
