using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveBrightnessChecker.Models
{
	/// <summary>
	/// MSMonitorClass Functions
	/// </summary>
	internal class MSMonitor
	{
		public static int GetBrightness()
		{
			using (var searcher = GetSearcher("WmiMonitorBrightness"))
			using (var instances = searcher.Get())
			{
				foreach (ManagementObject instance in instances)
				{
					return (byte)instance.GetPropertyValue("CurrentBrightness");
				}
				return -1;
			}
		}

		private static ManagementObjectSearcher GetSearcher(string className) =>
			new ManagementObjectSearcher(new ManagementScope(@"root\wmi"), new SelectQuery(className));
	}
}