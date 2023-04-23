using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Checker.Core.Helper;
using Checker.Core.ViewModels;

namespace Checker.Core.Views;

public sealed partial class MainPage : Page
{
	public static readonly Size DefaultSize = new(600, 360);

	#region Property

	public Brush SeparatorBrush
	{
		get => (Brush)GetValue(SeparatorBrushProperty);
		set => SetValue(SeparatorBrushProperty, value);
	}
	public static readonly DependencyProperty SeparatorBrushProperty =
		DependencyProperty.Register("SeparatorBrush", typeof(Brush), typeof(MainPage), new PropertyMetadata(default(Brush)));

	public bool IsCharted
	{
		get => (bool)GetValue(IsChartedProperty);
		set => SetValue(IsChartedProperty, value);
	}
	public static readonly DependencyProperty IsChartedProperty =
		DependencyProperty.Register("IsCharted", typeof(bool), typeof(MainPage), new PropertyMetadata(true));

	#endregion

	public MainPage()
	{
		this.InitializeComponent();
		this.DataContext = new MainPageViewModel(this);
		this.Loaded += OnLoaded;

		ApplicationView.PreferredLaunchViewSize = DefaultSize;
		ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

		if (OsVersion.Is11OrGreater)
		{
			BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
		}
		else
		{
			if (this.FindResource("AcrylicPageStyle") is Style acrylicPageStyle)
				this.Style = acrylicPageStyle;
		}

		// Hide default title bar.
		var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
		coreTitleBar.ExtendViewIntoTitleBar = true;
		coreTitleBar.LayoutMetricsChanged += OnLayoutMetricsChanged;

		var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
		appTitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
		appTitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

		// Set XAML element as a drag region.
		Window.Current.SetTitleBar(this.AppTitleBar);

		// Subscribe activated event.
		Window.Current.Activated += OnActivated;

		// Subscribe theme changed event.
		var Listener = new ThemeListener();
		Listener.ThemeChanged += OnThemeChanged;

		// Subscribe click event.
		this.ChartedButton.Click += OnChartedClick;

		UpdateSeparator();
		UpdateChartedButton();
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		await ((MainPageViewModel)this.DataContext).InitializeAsync();
	}

	private void OnLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
	{
		UpdateTitleBar(sender);
	}

	private void UpdateTitleBar(CoreApplicationViewTitleBar coreTitleBar)
	{
		// Update title bar control size as needed to account for system size changes.
		AppTitleBar.Height = coreTitleBar.Height;

		// Ensure the custom title bar does not overlap window caption controls.
		AppTitleBar.Margin = new Thickness(
			coreTitleBar.SystemOverlayLeftInset,
			AppTitleBar.Margin.Top,
			coreTitleBar.SystemOverlayRightInset,
			AppTitleBar.Margin.Bottom);
	}

	private void OnActivated(object sender, WindowActivatedEventArgs e)
	{
		UpdateTitleText(e);
	}

	private void UpdateTitleText(WindowActivatedEventArgs e)
	{
		var settings = new UISettings();
		var color = e.WindowActivationState switch
		{
			CoreWindowActivationState.CodeActivated or
			CoreWindowActivationState.PointerActivated => settings.UIElementColor(UIElementType.WindowText),
			_ => settings.UIElementColor(UIElementType.GrayText)
		};
		TitleTextBlock.Foreground = new SolidColorBrush(color);
	}

	private void OnThemeChanged(ThemeListener sender)
	{
		UpdateSeparator();
	}

	private void UpdateSeparator()
	{
		var color = Application.Current.RequestedTheme switch
		{
			ApplicationTheme.Dark => Windows.UI.Colors.Gray,
			_ => Windows.UI.Colors.Silver
		};
		SeparatorBrush = new SolidColorBrush(color);
	}

	private void OnChartedClick(object sender, RoutedEventArgs e)
	{
		IsCharted = !IsCharted;
		UpdateChartedButton();
	}

	private void UpdateChartedButton()
	{
		this.ChartedButton.Content = $"{(IsCharted ? "Hide" : "Show")} chart";
	}
}
