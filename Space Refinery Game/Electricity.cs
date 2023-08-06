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
	public static FaradayConstantUnit FaradayConstant => FaradayConstantUnit.Unit;

	/// <summary>
	/// [V] or [J/C]
	/// </summary>
	public static VoltageUnit Voltage => 1000;

	/// <summary>
	/// [J] of electrical energy to [C]
	/// </summary>
	/// <param name="electricalEnergy">[J]</param>
	/// <returns>[C]</returns>
	public static CoulombUnit ElectricalEnergyToCoulomb(EnergyUnit electricalEnergy)
	{
		return electricalEnergy / Voltage; // [J] / [J/C] => [C]
	}
}
