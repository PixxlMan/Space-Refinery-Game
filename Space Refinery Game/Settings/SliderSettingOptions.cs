using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public struct SliderSettingOptions : ISettingOptions
	{
		public SliderSettingOptions(FixedDecimalInt4 max, FixedDecimalInt4 min)
		{
			Max = max;
			Min = min;
		}

		public FixedDecimalInt4 Max;

		public FixedDecimalInt4 Min;
	}
}
