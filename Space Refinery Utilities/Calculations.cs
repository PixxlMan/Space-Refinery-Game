using Space_Refinery_Game;
using Space_Refinery_Utilities.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities;

public static class Calculations
{
	public static readonly DecimalNumber GasConstant = (DecimalNumber)8.314;

	public static PressureUnit CalculatePressureUsingIdealGasLaw(MolesUnit gasSubstanceAmount, TemperatureUnit averageTemperature, VolumeUnit volume)
	{
		// P * V = n * k * T
		// P = pressure [kg/m³]
		// V = volume [m³]
		// n = substance amount [mol]
		// k = gas constant (8.314 J K-1) [J/K⁻¹]
		// T = temperature [K]
		//
		// Solve for P:
		// P = (n * k * T) / V

		DecimalNumber gasMolesDecNum, avgTempDecNum, volDecNum;
		gasMolesDecNum = (DecimalNumber)gasSubstanceAmount;
		avgTempDecNum = (DecimalNumber)averageTemperature;
		volDecNum = (DecimalNumber)volume;

		PressureUnit pressure = new((gasMolesDecNum * GasConstant * avgTempDecNum) / (volDecNum));

		return pressure;
	}
}
