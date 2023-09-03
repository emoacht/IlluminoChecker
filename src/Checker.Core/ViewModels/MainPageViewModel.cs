using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Uwp;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

using Checker.Core.Models;
using Checker.Core.Views;

namespace Checker.Core.ViewModels;

public class MainPageViewModel : ObservableObject
{
	#region Property

	public double? Illuminance
	{
		get => _illuminance;
		private set => SetProperty(ref _illuminance, value);
	}
	private double? _illuminance;

	public int Brightness
	{
		get => _brightness;
		private set => SetProperty(ref _brightness, value);
	}
	private int _brightness;

	public TimeSpan IlluminanceUpdateInterval
	{
		get => LightInformation.ReportInterval;
		set => LightInformation.ReportInterval = value;
	}

	public TimeSpan BrightnessUpdateInterval
	{
		get => _brightnessWatcher.ReportInterval;
		set => _brightnessWatcher.ReportInterval = value;
	}

	public int DurationIndex
	{
		get
		{
			_durationIndex ??= SettingAccess.TryGetValue(out int value) ? value : 0;
			_duration = GetDuration(_durationIndex.Value);
			return _durationIndex.Value;
		}
		set
		{
			if (SetProperty(ref _durationIndex, value))
			{
				SettingAccess.SetValue(value);
				_duration = GetDuration(value);
			}
		}
	}
	private int? _durationIndex;
	private TimeSpan _duration = TimeSpan.FromMinutes(1);

	private static TimeSpan GetDuration(int index)
	{
		return index switch
		{
			1 => TimeSpan.FromMinutes(2),
			2 => TimeSpan.FromMinutes(3),
			_ => TimeSpan.FromMinutes(1)
		};
	}

	public double DurationOrigin
	{
		get => _durationOrigin;
		private set => SetProperty(ref _durationOrigin, value);
	}
	private double _durationOrigin = 0D;

	#endregion

	#region Command

	public ICommand PauseCommand => _pauseCommand ??= new RelayCommand(() => SwitchPaused());
	private RelayCommand _pauseCommand;

	public ICommand ExportCommand => _exportCommand ??= new AsyncRelayCommand(() => LogAccess.ExportAsync());
	private AsyncRelayCommand _exportCommand;

	#endregion

	private readonly MainPage _mainPage;
	private readonly BrightnessWatcher _brightnessWatcher;

	public MainPageViewModel(MainPage mainPage)
	{
		_mainPage = mainPage ?? throw new ArgumentNullException(nameof(mainPage));

		if (!LightInformation.TryGetAmbientLight(out float value))
			return;

		Illuminance = Round(value);
		LightInformation.AmbientLightChanged += OnAmbientLightChanged;

		_brightnessWatcher = new();
		Brightness = _brightnessWatcher.Brightness;
		_brightnessWatcher.BrightnessChanged += OnBrightnessChanged;
	}

	private async void OnAmbientLightChanged(object sender, float e)
	{
		Debug.Assert(!_mainPage.Dispatcher.HasThreadAccess);

		await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
		{
			Illuminance = Round(e);
			await UpdateCollectionsAsync(DateTimeOffset.Now);
		});
	}

	private async void OnBrightnessChanged(object sender, int e)
	{
		Debug.Assert(_mainPage.Dispatcher.HasThreadAccess);

		Brightness = e;
		await UpdateCollectionsAsync(DateTimeOffset.Now);
	}

	public Task InitializeAsync() => InitializeCollectionsAsync();

	private bool _isPaused;

	private void SwitchPaused()
	{
		if (_isPaused)
		{
			LightInformation.AmbientLightChanged += OnAmbientLightChanged;
			_brightnessWatcher.BrightnessChanged += OnBrightnessChanged;
			_isPaused = false;
		}
		else
		{
			LightInformation.AmbientLightChanged -= OnAmbientLightChanged;
			_brightnessWatcher.BrightnessChanged -= OnBrightnessChanged;
			_isPaused = true;
		}
	}

	#region Chart

	public SeriesCollection SeriesCollection { get; } = new();

	private readonly ChartValues<ObservablePoint> _illuminanceCollection = new();
	private readonly ChartValues<ObservablePoint> _brightnessCollection = new();
	private DateTimeOffset _startTime;

	private async Task InitializeCollectionsAsync()
	{
		if (!Illuminance.HasValue)
			return;

		// https://v0.lvcharts.com/App/examples/v1/Wpf/Line

		SeriesCollection.Add(new LineSeries
		{
			Title = nameof(Illuminance),
			Values = _illuminanceCollection,
		});
		SeriesCollection.Add(new LineSeries
		{
			Title = nameof(Brightness),
			Values = _brightnessCollection,
			PointGeometry = DefaultGeometries.Square,
			Fill = new SolidColorBrush(Colors.Transparent),
		});

		await LogAccess.CleanAsync(TimeSpan.FromDays(7));

		_startTime = DateTimeOffset.Now;
		await UpdateCollectionsAsync(_startTime);
	}

	private async Task UpdateCollectionsAsync(DateTimeOffset time)
	{
		var elapsed = Round((time - _startTime).TotalSeconds);

		UpdateCollection(_illuminanceCollection, Illuminance.Value, elapsed);
		UpdateCollection(_brightnessCollection, Brightness, elapsed);

		DurationOrigin = (_illuminanceCollection.Count > 1) ? double.NaN : 0D;

		await LogAccess.RecordAsync(time, elapsed, Illuminance.Value, Brightness);

		void UpdateCollection(ChartValues<ObservablePoint> collection, double value, double elapsed)
		{
			while ((collection.Count > 0)
				&& (elapsed - collection[0].X >= _duration.TotalSeconds))
			{
				collection.RemoveAt(0);
			}
			collection.Add(new ObservablePoint(elapsed, value));
		}
	}

	#endregion

	private static double Round(double value) => Math.Round(value * 100D, MidpointRounding.AwayFromZero) / 100D;
}
