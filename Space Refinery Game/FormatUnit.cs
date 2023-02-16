using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game;

public static class FormatUnit
{
	/// <summary>
	/// Formats a mass in kilograms according to player preferences.
	/// </summary>
	/// <param name="mass">[kg]</param>
	/// <returns>Formatted weight</returns>
	public static string FormatMass(this DecimalNumber mass)
	{
		return $"{mass.ToString(decimals: 2)} kg";
	}
	
	/// <summary>
	/// Formats a volume in cubic meters according to player preferences.
	/// </summary>
	/// <param name="volume">[m³]</param>
	/// <returns>Formatted volume</returns>
	public static string FormatVolume(this DecimalNumber volume)
	{
		return $"{volume.ToString(decimals: 2)} m³";
	}

	/// <summary>
	/// Formats a pressure in pascal according to player preferences.
	/// </summary>
	/// <param name="pessure">[kg/m³]</param>
	/// <returns>Formatted pressure</returns>
	public static string FormatPressure(this DecimalNumber pressure)
	{
		return $"{pressure.ToString(decimals: 2)} kg/m³";
	}

	/// <summary>
	/// Formats a distance in meters according to player preferences.
	/// </summary>
	/// <param name="distance">[m]</param>
	/// <returns>Formatted distance</returns>
	public static string FormatDistance(this DecimalNumber distance)
	{
		return $"{distance.ToString(decimals: 2)} m";
	}

	/// <summary>
	/// Formats substance amount in moles according to player preferences.
	/// </summary>
	/// <param name="substanceAmount">[mol]</param>
	/// <returns>Formatted substance amount</returns>
	public static string FormatSubstanceAmount(this DecimalNumber substanceAmount)
	{
		return $"{substanceAmount.ToString(decimals: 2)} mol";
	}

	/// <summary>
	/// Formats density in kilograms per cubic meter according to player preferences.
	/// </summary>
	/// <param name="density">[kg/m³]</param>
	/// <returns>Formatted density</returns>
	public static string FormatDensity(this DecimalNumber density)
	{
		return $"{density.ToString(decimals: 2)} kg/m³";
	}

	/// <summary>
	/// Formats specific heat capacity in joules per kilogram according to player preferences.
	/// </summary>
	/// <param name="specificHeatCapacity">[J/kg]</param>
	/// <returns>Formatted specific heat capacity</returns>
	public static string FormatSpecificHeatCapacity(this DecimalNumber specificHeatCapacity)
	{
		return $"{specificHeatCapacity.ToString(decimals: 2)} J/kg";
	}
	
	/// <summary>
	/// Formats temperature in kelvin according to player preferences.
	/// </summary>
	/// <param name="temperature">[K]</param>
	/// <returns>Formatted temperature</returns>
	public static string FormatTemperature(this DecimalNumber temperature)
	{
		return $"{temperature.ToString(decimals: 2)} K";
	}

	/// <summary>
	/// Formats energy in joules according to player preferences.
	/// </summary>
	/// <param name="temperature">[J]</param>
	/// <returns>Formatted energy</returns>
	public static string FormatEnergy(this DecimalNumber energy)
	{
		return $"{energy.ToString(decimals: 2)} J";
	}

	/// <summary>
	/// Formats a value between zero and one as a percentage according to player preferences.
	/// Values greater than one or smaller than zero produce a percentage larger than 100 % or smaller than 0 %.
	/// </summary>
	/// <param name="value">0 <= value <= 1</param>
	/// <returns>Formatted percentage</returns>
	public static string FormatPercentage(this DecimalNumber value)
	{
		return $"{Math.Round((value * 100).ToDecimal(), 1)} %%";
	}
}
