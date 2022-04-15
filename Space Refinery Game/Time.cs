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
		public static readonly FixedDecimalInt4 TickInterval = (FixedDecimalInt4)0.016666;

		public static readonly FixedDecimalInt4 UpdateInterval = (FixedDecimalInt4)0.005;

		public static readonly FixedDecimalInt4 PhysicsInterval = (FixedDecimalInt4)0.016666;
	}
}
