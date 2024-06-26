﻿using System.Diagnostics;

namespace Space_Refinery_Engine;

public static class Time // https://fpstoms.com/
{
	/// <summary>
	/// [TPS] or [Tick/s]
	/// <para/>
	/// Equals 50 tps.
	/// </summary>
	public static readonly RateUnit TickRate = 50; // 50 tps (Ticks per second.)

	public static readonly IntervalUnit TickInterval = IntervalRateConversionUnit.Unit / TickRate; // 50 tps (Ticks per second.)

	public static readonly RateUnit UpdateRate = 200; // 200 ups

	public static readonly IntervalUnit UpdateInterval = IntervalRateConversionUnit.Unit / UpdateRate;

	public static readonly RateUnit PhysicsRate = 150; // 150 pups

	public static readonly IntervalUnit PhysicsInterval = IntervalRateConversionUnit.Unit / PhysicsRate;

	public static long TicksElapsed = 0; // TODO: Provide property that reads this using volatile or interlocked reads? That way the value might be outdated at worst, never corrupted.

	public static DecimalNumber CurrentTickTime => TicksElapsed * (DecimalNumber)TickInterval;

	public static void WaitIntervalLimit(IntervalUnit intervalTime, TimeUnit intervalStartTime, Stopwatch stopwatch, out TimeUnit timeOfContinuation)
	{
		TimeUnit timeToStopWaiting = intervalStartTime + intervalTime;
		while (stopwatch.Elapsed.TotalSeconds < timeToStopWaiting)
		{
			if (timeToStopWaiting - stopwatch.Elapsed.TotalSeconds > 10 * (TimeUnit)DN.Milli)
			{
				Thread.Sleep(6);
			}
			else if (timeToStopWaiting - stopwatch.Elapsed.TotalSeconds > 2 * (TimeUnit)DN.Milli)
			{
				Thread.Yield();
			}
			else
			{
				Thread.SpinWait(4);
			}
		}

		timeOfContinuation = timeToStopWaiting;
	}

	public static void Reset()
	{
		TicksElapsed = 0;
	}

	public static string ResponseSpinner(TimeUnit time)
	{
		return "|/-\\"[(int)((DecimalNumber)time / 0.05) & 3].ToString(); // https://github.com/ocornut/imgui/issues/1901#issuecomment-400563921
	}

	[Conditional("DEBUG")]
	public static void ResponseSpinner(TimeUnit time, ref string spinnerString)
	{
		//lock (spinnerString) // C# seems to complain about locking it. I'm suuure it's fiiine.
			spinnerString = ResponseSpinner(time);
	}
}
