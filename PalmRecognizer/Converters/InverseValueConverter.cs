﻿namespace PalmRecognizer.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;

	class InverseValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? false : true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}