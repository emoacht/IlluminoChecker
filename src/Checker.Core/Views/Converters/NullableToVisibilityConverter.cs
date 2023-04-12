using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Checker.Core.Views.Converters;

public class NullableToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (!Enum.TryParse<Visibility>(parameter?.ToString(), out var visibility))
			return DependencyProperty.UnsetValue;

		return (visibility, value) switch
		{
			(Visibility.Visible, not null) => Visibility.Visible,
			(Visibility.Visible, null) => Visibility.Collapsed,
			(Visibility.Collapsed, not null) => Visibility.Collapsed,
			(Visibility.Collapsed, null) => Visibility.Visible,
			_ => DependencyProperty.UnsetValue
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotSupportedException();
	}
}
