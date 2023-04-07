using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Checker.Uwp.Views;

namespace Checker.Uwp;

sealed partial class App : Application
{
	public App()
	{
		this.InitializeComponent();
		this.Suspending += OnSuspending;
	}

	protected override void OnLaunched(LaunchActivatedEventArgs e)
	{
		ApplicationView.GetForCurrentView().TryResizeView(MainPage.DefaultSize);

		var rootFrame = Window.Current.Content as Frame;
		if (rootFrame is null)
		{
			rootFrame = new Frame();

			rootFrame.NavigationFailed += OnNavigationFailed;

			if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
			{
				//TODO: Load state from previously suspended application
			}

			Window.Current.Content = rootFrame;
		}

		if (e.PrelaunchActivated == false)
		{
			if (rootFrame.Content == null)
			{
				rootFrame.Navigate(typeof(MainPage), e.Arguments);
			}

			Window.Current.Activate();
		}
	}

	protected override void OnActivated(IActivatedEventArgs e)
	{
		var rootFrame = Window.Current.Content as Frame;
		if (rootFrame is null)
		{
			rootFrame = new Frame();
			rootFrame.NavigationFailed += OnNavigationFailed;

			if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
			{
				//TODO: Load state from previously suspended application
			}

			Window.Current.Content = rootFrame;
		}

		if (rootFrame.Content is null)
		{
			rootFrame.Navigate(typeof(MainPage), null);
		}

		Window.Current.Activate();
	}

	private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
	{
		throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
	}

	private void OnSuspending(object sender, SuspendingEventArgs e)
	{
		var deferral = e.SuspendingOperation.GetDeferral();
		//TODO: Save application state and stop any background activity
		deferral.Complete();
	}
}
