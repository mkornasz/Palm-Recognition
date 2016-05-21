namespace PalmRecognizer.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    class BoolToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "1*" : "0*";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}