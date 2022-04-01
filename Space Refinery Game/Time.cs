using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public static class Time
	{
		public static readonly FixedDecimalInt4 TickInterval = 0.0166f;

		public static readonly FixedDecimalInt4 UpdateInterval = 0.0050f;

		public static readonly FixedDecimalInt4 PhysicsInterval = 0.0166f;
	}
}
