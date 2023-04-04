using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Checker.Wpf;

[ValueConversion(typeof(bool), typeof(string))]
public class BooleanToStringConverter : IValueConverter
{
	public string TrueString { get; set; }
	public string FalseString { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not bool sourceValue)
			return DependencyProperty.UnsetValue;

		return sourceValue
			? (TrueString ?? true.ToString())
			: (FalseString ?? false.ToString());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}