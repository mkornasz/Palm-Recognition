using System;
using System.Globalization;
using System.Windows.Data;

namespace PalmRecognizer.Converters
{
    class DistanceStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double valueDouble = (double)value;
            return "Distance: " + valueDouble.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
