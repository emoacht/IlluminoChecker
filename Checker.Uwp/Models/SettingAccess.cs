using System;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Checker.Uwp.Models;

internal static class SettingAccess
{
	private static readonly Lazy<IPropertySet> _values = new(() => ApplicationData.Current.LocalSettings.Values);
	private static readonly object _lock = new();

	public static bool TryGetValue<T>(out T propertyValue, [CallerMemberName] string propertyName = null)
	{
		lock (_lock)
		{
			if (_values.Value.TryGetValue(propertyName, out object value))
			{
				propertyValue = (T)value;
				return true;
			}
			propertyValue = default;
			return false;
		}
	}

	public static void SetValue<T>(T propertyValue, [CallerMemberName] string propertyName = null)
	{
		lock (_lock)
		{
			// Add or change value.
			_values.Value[propertyName] = propertyValue;
		}
	}

	public static bool RemoveValue([CallerMemberName] string propertyName = null)
	{
		lock (_lock)
		{
			return _values.Value.Remove(propertyName);
		}
	}
}
