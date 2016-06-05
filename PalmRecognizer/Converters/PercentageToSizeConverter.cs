namespace PalmRecognizer.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;

	internal class PercentageToSizeConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			double percent = double.Parse(values[0].ToString());
			double size = double.Parse(values[1].ToString());

			double param;
			if (parameter != null && double.TryParse(parameter.ToString(), out param)) return percent * size - param;

			return percent * size;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
