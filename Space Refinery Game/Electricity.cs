using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game;

public static class Electricity
{
	/// <summary>
	/// [C/mol]
	/// </summary>
	public static DecimalNumber FaradayConstant => 96485; // [C/mol]

	/// <summary>
	/// [V], [J/C]
	/// </summary>
	public static VoltageUnit Voltage => 1000; // [V] or [J/C]

	/// <summary>
	/// [J] of electrical energy to [C]
	/// </summary>
	/// <param name="electricalEnergy">[J]</param>
	/// <returns>[C]</returns>
	public static CoulombUnit ElectricalEnergyToCoulomb(EnergyUnit electricalEnergy /* [J] */)
	{
		return electricalEnergy * Voltage; // [J] * [J/C] => [C]
	}
}
