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
		public SliderSettingOptions(DecimalNumber max, DecimalNumber min, string label = "")
		{
			Max = max;
			Min = min;
			Label = label;
		}

		public DecimalNumber Max;

		public DecimalNumber Min;

		public string Label;
	}
}
