using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public static class Electricity
	{
		public static DecimalNumber Voltage => 1000; // V or J/C

		public static DecimalNumber ElectricalEnergyToCoulomb(DecimalNumber electricalEnergy /* J */)
		{
			return electricalEnergy * Voltage; // J * J/C => C
		}
	}
}
