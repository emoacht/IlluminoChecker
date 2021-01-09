using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace AdaptiveBrightnessChecker.Models
{
	internal sealed class PowerWatcher : IDisposable
	{
		#region Win32

		[DllImport("User32.dll", SetLastError = true)]
		private static extern IntPtr RegisterPowerSettingNotification(
			IntPtr hRecipient,
			[MarshalAs(UnmanagedType.LPStruct), In] Guid PowerSettingGuid,
			uint Flags);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnregisterPowerSettingNotification(
			IntPtr Handle);

		[StructLayout(LayoutKind.Sequential)]
		private struct POWERBROADCAST_SETTING
		{
			public Guid PowerSetting;
			public uint DataLength;
			public IntPtr Data;
		}

		private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
		private const int WM_POWERBROADCAST = 0x0218;
		private const int PBT_POWERSETTINGCHANGE = 0x8013;

		#endregion

		public event EventHandler<PowerSettingChangedEventArgs> PowerSettingChanged;

		private readonly Window _window;
		private IntPtr _windowHandle;
		private HwndSource _windowSource;

		private readonly Guid[] _settingGuids;

		public PowerWatcher(Window window, params Guid[] settingGuids)
		{
			this._window = window ?? throw new ArgumentNullException(nameof(window));
			this._window.SourceInitialized += OnSourceInitialized;

			this._settingGuids = settingGuids?.ToArray() ?? throw new ArgumentNullException(nameof(settingGuids));
		}

		private void OnSourceInitialized(object sender, EventArgs e)
		{
			_windowHandle = new WindowInteropHelper(_window).Handle;
			_windowSource = HwndSource.FromHwnd(_windowHandle);
			_windowSource.AddHook(WndProc);

			_settingGuids.ToList().ForEach(x => RegisterPowerSettingEvent(x));
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_POWERBROADCAST:
					if (wParam.ToInt32() == PBT_POWERSETTINGCHANGE)
					{
						var data = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(lParam);
						var buffer = (data.DataLength == 4 /* DWORD */) ? data.Data.ToInt32() : 0;
						PowerSettingChanged?.Invoke(_window, new PowerSettingChangedEventArgs(data.PowerSetting, buffer));
					}
					break;
			}
			return IntPtr.Zero;
		}

		#region Register/Unregister

		private readonly List<IntPtr> _registrationHandles = new List<IntPtr>();

		private bool RegisterPowerSettingEvent(Guid settingGuid)
		{
			if (_windowHandle == IntPtr.Zero)
				throw new InvalidOperationException("The window handle must be given.");

			var handle = RegisterPowerSettingNotification(
				_windowHandle,
				settingGuid,
				DEVICE_NOTIFY_WINDOW_HANDLE);
			if (handle != IntPtr.Zero)
			{
				_registrationHandles.Add(handle);
				return true;
			}
			return false;
		}

		private void UnregisterPowerSettingEvent()
		{
			PowerSettingChanged = null;

			foreach (var handle in _registrationHandles)
				UnregisterPowerSettingNotification(handle);

			_registrationHandles.Clear();
		}

		#endregion

		#region Dispose

		private bool _isDisposed = false;

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			_windowSource.RemoveHook(WndProc);
			UnregisterPowerSettingEvent();
		}

		#endregion
	}

	public class PowerSettingChangedEventArgs : EventArgs
	{
		public Guid Guid { get; }
		public int Data { get; }

		public PowerSettingChangedEventArgs(Guid guid, int data) => (Guid, Data) = (guid, data);
	}
}