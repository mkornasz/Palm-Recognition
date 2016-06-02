using System;
using System.Globalization;
using System.Windows.Data;

namespace PalmRecognizer.Converters
{
    class SimilarityStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double valueDouble = (double)value;
            return valueDouble.ToString() + " %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
