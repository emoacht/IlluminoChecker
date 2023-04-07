using System.Diagnostics;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

	private void SetSeparatorBrush()
	{
		var color = Application.Current.RequestedTheme switch
		{
			ApplicationTheme.Dark => Windows.UI.Colors.Gray,
			_ => Windows.UI.Colors.Silver
		};
		SeparatorBrush = new SolidColorBrush(color);
	}

	#endregion

	public MainPage()
	{
		this.InitializeComponent();
		this.DataContext = new MainPageViewModel(this);
		this.Loaded += OnLoaded;

		ApplicationView.PreferredLaunchViewSize = DefaultSize;
		ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

		// Hide default title bar.
		var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
		coreTitleBar.ExtendViewIntoTitleBar = true;
		coreTitleBar.LayoutMetricsChanged += OnLayoutMetricsChanged;

		var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
		appTitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
		appTitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

		// Set XAML element as a drag region.
		Window.Current.SetTitleBar(AppTitleBar);

		// Subscribe activated event.
		Window.Current.Activated += OnActivated;

		// Subscribe theme changed event.
		var Listener = new ThemeListener();
		Listener.ThemeChanged += Listener_ThemeChanged;

		SetSeparatorBrush();
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		await ((MainPageViewModel)this.DataContext).InitializeAsync();
	}

	private void OnLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
	{
		UpdateTitleBarLayout(sender);
	}

	private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
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
		Debug.WriteLine(e.WindowActivationState);
	}

	private void Listener_ThemeChanged(ThemeListener sender)
	{
		Debug.WriteLine(Application.Current.RequestedTheme);
		SetSeparatorBrush();
	}
}
