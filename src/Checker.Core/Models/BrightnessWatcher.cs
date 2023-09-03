using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace Checker.Core.Models;

internal class BrightnessWatcher
{
	public int Brightness { get; private set; }

	private bool UpdateBrightness(BrightnessOverride instance)
	{
		double value = instance.GetLevelForScenario(DisplayBrightnessScenario.DefaultBrightness);
		int brightness = (int)(value * 100D);
		if (Brightness == brightness)
			return false;

		Brightness = brightness;
		_brightnessChanged?.Invoke(this, Brightness);
		return true;
	}

	private readonly BrightnessOverride _instance;
	private readonly DispatcherTimer _timer;

	public BrightnessWatcher()
	{
		_instance = BrightnessOverride.GetForCurrentView();
		UpdateBrightness(_instance);

		// BrightnessOverride.BrightnessLevelChanged event does not work for capturing adaptive brightness.

		_timer = new DispatcherTimer();
		_timer.Interval = ReportInterval;
		_timer.Tick += OnTick;
		_timer.Start();
	}

	private void OnTick(object sender, object e)
	{
		_timer.Stop();
		UpdateBrightness(_instance);
		_timer.Start();
	}

	private readonly object _lock = new();

	/// <summary>
	/// Report interval of brightness level
	/// </summary>
	/// <remarks>To revert to default, set TimeSpan.Zero.</remarks>
	public TimeSpan ReportInterval
	{
		get => _timer?.Interval ?? _defaultReportInterval;
		set
		{
			lock (_lock)
			{
				var isEnabled = _timer.IsEnabled;
				_timer.Stop();

				_timer.Interval = (value <= TimeSpan.Zero)
					? _defaultReportInterval
					: value;

				if (isEnabled)
					_timer.Start();
			}
		}
	}
	private readonly TimeSpan _defaultReportInterval = TimeSpan.FromSeconds(1);

	/// <summary>
	/// Occurs when brightness level has changed.
	/// </summary>
	/// <remarks>EventArgs indicates brightness level in percentage.</remarks>
	public event EventHandler<int> BrightnessChanged
	{
		add
		{
			lock (_lock)
			{
				_brightnessChanged += value;
				if (_brightnessChanged.GetInvocationList().Length == 1)
					_timer.Start();
			}
		}
		remove
		{
			lock (_lock)
			{
				_brightnessChanged -= value;
				if (_brightnessChanged is null)
					_timer.Stop();
			}
		}
	}
	private event EventHandler<int> _brightnessChanged;
}
