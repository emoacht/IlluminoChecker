using System;

namespace Checker.Core.Helper;

public static class OsVersion
{
	/// <summary>
	/// Whether OS is Windows 11 (10.0.22000) or greater
	/// </summary>
	public static bool Is11OrGreater => (_is11OrGreater ??= IsEqualToOrGreaterThan(10, 0, 22000));
	private static bool? _is11OrGreater;

	private static bool IsEqualToOrGreaterThan(in int major, in int minor = 0, in int build = 0)
	{
		return (new Version(major, minor, build) <= Environment.OSVersion.Version);
	}
}
