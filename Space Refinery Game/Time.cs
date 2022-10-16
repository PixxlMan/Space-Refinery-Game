using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public static class Time // https://fpstoms.com/
	{
		public static readonly FixedDecimalLong8 TickInterval = (FixedDecimalLong8)0.02; // 50 tps

		public static readonly FixedDecimalLong8 UpdateInterval = (FixedDecimalLong8)0.005; // 200 ups

		public static readonly FixedDecimalLong8 PhysicsInterval = (FixedDecimalLong8)0.016666; // 60 pups
	}
}
