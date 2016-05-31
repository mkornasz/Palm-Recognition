using System;
using System.Globalization;
using System.Windows.Data;

namespace PalmRecognizer.Converters
{
    class PalmDefectLenghtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value;
            return (int)doubleValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
