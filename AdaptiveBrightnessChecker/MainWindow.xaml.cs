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

namespace AdaptiveBrightnessChecker
{
	public partial class MainWindow : Window
	{
		private readonly LightSensor _sensor;

		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += OnLoaded;

			_sensor = LightSensor.GetDefault();
			var reading = _sensor?.GetCurrentReading();
			if (reading != null)
			{
				Illuminance = (int)reading.IlluminanceInLux;

				_sensor.ReadingChanged += (sender, args) =>
				{
					this.Dispatcher.Invoke(() => Illuminance = (int)args.Reading.IlluminanceInLux);
				};
			}

			var isEnabled = PowerManagement.IsActiveSchemeAdaptiveBrightnessEnabled();
			if (isEnabled.HasValue)
			{
				AdaptiveBrightness = isEnabled.Value ? "On" : "Off";
			}
		}

		#region Property

		public int Illuminance
		{
			get { return (int)GetValue(IlluminanceProperty); }
			set { SetValue(IlluminanceProperty, value); }
		}
		public static readonly DependencyProperty IlluminanceProperty =
			DependencyProperty.Register(
				"Illuminance",
				typeof(int),
				typeof(MainWindow),
				new PropertyMetadata(-1));

		public string AdaptiveBrightness
		{
			get { return (string)GetValue(AdaptiveBrightnessProperty); }
			set { SetValue(AdaptiveBrightnessProperty, value); }
		}
		public static readonly DependencyProperty AdaptiveBrightnessProperty =
			DependencyProperty.Register(
				"AdaptiveBrightness",
				typeof(string),
				typeof(MainWindow),
				new PropertyMetadata(null));

		public bool IsReady
		{
			get { return (bool)GetValue(IsReadyProperty); }
			set { SetValue(IsReadyProperty, value); }
		}
		public static readonly DependencyProperty IsReadyProperty =
			DependencyProperty.Register(
				"IsReady",
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
		private readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (Environment.GetCommandLineArgs().Skip(1).Contains(StartArguments))
				await CheckAsync();
		}

		private async void CheckButton_Click(object sender, RoutedEventArgs e)
		{
			await CheckAsync();
		}

		private async Task CheckAsync()
		{
			try
			{
				IsReady = false;

				Debug.WriteLine("[Start]");

				var startBrightness = PowerManagement.GetActiveSchemeBrightness();
				var startTime = DateTime.Now;

				var result = await IncrementBrightnessAsync();
				Debug.WriteLine(result);

				PowerManagement.SetActiveSchemeBrightness(startBrightness);

				await Task.Run(() => File.WriteAllText(
					$"{startTime:MMddhhmmss}.txt",
					$"{startTime:MM/dd hh:mm:ss}" + Environment.NewLine + result));

				Debug.WriteLine("[End]");

				await Task.Delay(Interval);
				Status = null;
			}
			finally
			{
				IsReady = true;
			}
		}

		private async Task<string> IncrementBrightnessAsync()
		{
			var sb = new StringBuilder();
			sb.AppendLine("Illuminance, Original, Adjusted");

			for (int brightness = 0; brightness <= 100; brightness++)
			{
				if (!PowerManagement.SetActiveSchemeBrightness(brightness))
					break;

				await Task.Delay(Interval);

				var setting = PowerManagement.GetActiveSchemeBrightness();
				var actual = MSMonitor.GetBrightness();
				Status = $"Illuminance: {Illuminance}, Original: {setting}, Adjusted: {actual}";
				Debug.WriteLine(Status);

				sb.AppendLine($"{Illuminance}, {setting}, {actual}");
			}

			return sb.ToString();
		}
	}
}