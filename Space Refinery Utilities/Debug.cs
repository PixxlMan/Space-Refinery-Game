using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities
{
	/// <summary>
	/// Debugging utility for putting the debugger inside the Tick method of a desired entity.
	/// </summary>
	public static class DebugStopPoints
	{
		static Guid guidToStop;

#if DEBUG
		public static void RegisterStopPoint(Guid guid)
		{
			guidToStop = guid;
		}
#endif

		[DebuggerHidden]
		public static void TickStopPoint(Guid guid)
		{
#if DEBUG
			if (guid == guidToStop)
			{
				Debugger.Break();
			}
#endif
		}
	}
}
