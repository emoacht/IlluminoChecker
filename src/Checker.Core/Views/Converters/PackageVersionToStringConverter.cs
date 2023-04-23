using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Data;

namespace Checker.Core.Views.Converters;

public class PackageVersionToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		return value switch
		{
			PackageVersion v => $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}",
			_ => value.ToString()
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotSupportedException();
	}
}
