using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Devices.Sensors;

using AdaptiveBrightnessChecker.Models;

namespace AdaptiveBrightnessChecker
{
	public partial class MainWindow : Window
	{
		private readonly LightSensor _sensor;
		private readonly PowerWatcher _watcher;

		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += OnLoaded;

			_sensor = LightSensor.GetDefault();
			if (_sensor != null)
			{
				var reading = _sensor.GetCurrentReading();
				if (reading != null)
					AmbientLight = (int)reading.IlluminanceInLux;

				_sensor.ReadingChanged += (sender, e) =>
				{
					this.Dispatcher.Invoke(() => AmbientLight = (int)e.Reading.IlluminanceInLux);
				};
			}

			if (PowerManagement.IsActiveSchemeAdaptiveBrightnessEnabled() == true)
				IsAdaptiveBrightnessEnabled = true;

			_watcher = new PowerWatcher(this, PowerManagement.VIDEO_ADAPTIVE_DISPLAY_BRIGHTNESS);
			_watcher.PowerSettingChanged += (sender, e) =>
			{
				// 0: Off
				// 1: On
				IsAdaptiveBrightnessEnabled = (e.Data == 1);
			};
		}

		#region Property

		public int AmbientLight
		{
			get { return (int)GetValue(AmbientLightProperty); }
			set { SetValue(AmbientLightProperty, value); }
		}
		public static readonly DependencyProperty AmbientLightProperty =
			DependencyProperty.Register(
				"AmbientLight",
				typeof(int),
				typeof(MainWindow),
				new PropertyMetadata(-1));

		public bool IsAdaptiveBrightnessEnabled
		{
			get { return (bool)GetValue(IsAdaptiveBrightnessEnabledProperty); }
			set { SetValue(IsAdaptiveBrightnessEnabledProperty, value); }
		}
		public static readonly DependencyProperty IsAdaptiveBrightnessEnabledProperty =
			DependencyProperty.Register(
				"IsAdaptiveBrightnessEnabled",
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(false));

		public bool CanCheck
		{
			get { return (bool)GetValue(CanCheckProperty); }
			set { SetValue(CanCheckProperty, value); }
		}
		public static readonly DependencyProperty CanCheckProperty =
			DependencyProperty.Register(
				"CanCheck",
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(true));

		public string Status
		{
			get { return (string)GetValue(StatusProperty); }
			set { SetValue(StatusProperty, value); }
		}
		public static readonly DependencyProperty StatusProperty =
			DependencyProperty.Register(
				"Status",
				typeof(string),
				typeof(MainWindow),
				new PropertyMetadata(null));

		#endregion

		private const string StartArguments = "/start";

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (Environment.GetCommandLineArgs().Skip(1).Contains(StartArguments))
				await CheckAsync();
		}

		private async void Check(object sender, RoutedEventArgs e) => await CheckAsync();

		private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(500);

		private async Task CheckAsync()
		{
			try
			{
				CanCheck = false;

				var startBrightness = PowerManagement.GetActiveSchemeBrightness();
				var startTime = DateTime.Now;

				var buffer = new StringBuilder("Ambient Light, Original, Adjusted");

				for (int brightness = 0; brightness <= 100; brightness++)
				{
					if (!PowerManagement.SetActiveSchemeBrightness(brightness))
						break;

					await Task.Delay(_interval);

					var original = PowerManagement.GetActiveSchemeBrightness();
					var actual = MSMonitor.GetBrightness();

					Status = $"Ambient Light: {AmbientLight}, Original: {original}, Adjusted: {actual}";
					Debug.WriteLine(Status);

					buffer.AppendLine($"{AmbientLight}, {original}, {actual}");
				}

				PowerManagement.SetActiveSchemeBrightness(startBrightness);

				await Task.Run(() => File.WriteAllText(
					$"{startTime:MMddhhmmss}.txt",
					$"{startTime:MM/dd hh:mm:ss}" + Environment.NewLine + buffer.ToString()));

				Status = null;
			}
			finally
			{
				CanCheck = true;
			}
		}
	}
}