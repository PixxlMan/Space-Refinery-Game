using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities;

/// <summary>
/// Debugging utility for putting the debugger inside the Tick method of a desired entity.
/// </summary>
public static class DebugStopPoints
{
	static object objectToStop;

	static object syncRoot = new();

#if DEBUG
	public static void RegisterStopPoint(object objectToStop)
	{
		lock (syncRoot)
		{
			DebugStopPoints.objectToStop = objectToStop;
		}
	}
#endif

	[DebuggerHidden]
	public static void TickStopPoint(object obj)
	{
#if DEBUG
		lock (syncRoot)
		{
			if (ReferenceEquals(objectToStop, obj))
			{
				obj = syncRoot; // Make sure to only stop once.
				Debugger.Break();
			}
		}
#endif
	}
}
