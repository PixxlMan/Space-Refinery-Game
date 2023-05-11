using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities;

public static class InterlockedExtensions
{
	public static int InterlockedReadInt(ref int location)
	{
		return Interlocked.Add(ref location, 0);
	}
}
