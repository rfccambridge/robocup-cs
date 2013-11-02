using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace RFC.Utilities
{
	/// <summary>
	/// A class that measures time at a much higher resolution than the standard C# timers.
	/// Uses Windows API calls, so it most likely will not work on anything but Windows.
	/// 
	/// Resolution is system-dependent.
	/// 
	/// Downloaded from http://www.eggheadcafe.com/articles/20021111.asp
	/// </summary>
	public class HighResTimer
	{
		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);

		static private HighResTimer start_timer = new HighResTimer();
		static HighResTimer()
		{
			start_timer.Start();
		}
		/// <summary>
		/// Returns the number of seconds since time started being tracked (which is guaranteed to be before
		/// any calls to SecondsSinceStart, but no guarantees about when it is compared to the start of the program).
		/// </summary>
		static public double SecondsSinceStart() {
			start_timer.Stop();
			return start_timer.Duration;
		}

		private long startTime;
		private long stopTime;
		private long freq;
		/// <summary>
		/// ctor
		/// </summary>
		public HighResTimer()
		{
			startTime = 0;
			stopTime = 0;
			freq = 0;
			if (QueryPerformanceFrequency(out freq) == false)
			{
				throw new Win32Exception(); // timer not supported
			}
		}
		/// <summary>
		/// Start the timer
		/// </summary>
		/// <returns>long - tick count</returns>
		public long Start()
		{
			QueryPerformanceCounter(out startTime);
			return startTime;
		}
		/// <summary>
		/// Stop timer
		/// </summary>
		/// <returns>long - tick count</returns>
		public long Stop()
		{
			QueryPerformanceCounter(out stopTime);
			return stopTime;
		}
		/// <summary>
		/// Return the duration of the timer (in seconds)
		/// </summary>
		/// <returns>double - duration</returns>
		public double Duration
		{
			get
			{
				return (double)(stopTime - startTime) / (double)freq;
			}
		}
		/// <summary>
		/// Frequency of timer (no counts in one second on this machine)
		/// </summary>
		///<returns>long - Frequency</returns>
		public long Frequency
		{
			get
			{
				QueryPerformanceFrequency(out freq);
				return freq;
			}
		}
	}
}
