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
		public SliderSettingOptions(DecimalNumber max, DecimalNumber min)
		{
			Max = max;
			Min = min;
		}

		public DecimalNumber Max;

		public DecimalNumber Min;
	}
}
