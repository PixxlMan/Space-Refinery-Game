using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public static class Time // https://fpstoms.com/
	{
		public static readonly FixedDecimalLong8 TickInterval = 1 / (FixedDecimalLong8)50; // 50 tps

		public static readonly FixedDecimalLong8 UpdateInterval = 1 / (FixedDecimalLong8)200; // 200 ups

		public static readonly FixedDecimalLong8 PhysicsInterval = 1 / (FixedDecimalLong8)60; // 60 pups

		public static long TicksElapsed = 0;

		public static DecimalNumber CurrentTickTime => TicksElapsed * TickInterval;

		public static void WaitIntervalLimit(FixedDecimalLong8 intervalTime, FixedDecimalLong8 intervalStartTime, Stopwatch stopwatch, out FixedDecimalLong8 timeOfContinuation)
		{
			FixedDecimalLong8 timeToStopWaiting = intervalStartTime + intervalTime;
			while (stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>() < timeToStopWaiting)
			{
				Thread.SpinWait(4);
			}

			timeOfContinuation = timeToStopWaiting;
		}
	}
}
