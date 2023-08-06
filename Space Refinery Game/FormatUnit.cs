using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game;

public static class FormatUnit
{
	public static void RegisterToSettings(Settings settings)
	{
		settings.RegisterToSettingValue<SwitchSettingValue>("Use Celcius", (v) => useCelcius = v);
	}

	static bool useCelcius = false;

	/// <summary>
	/// Formats a mass in kilograms according to player preferences.
	/// </summary>
	/// <param name="massUnit">[kg]</param>
	/// <returns>Formatted weight</returns>
	public static string FormatMass(this MassUnit massUnit)
	{
		var mass = (DecimalNumber)massUnit;
		return $"{mass.ToString(decimals: 2)} kg";
	}

	/// <summary>
	/// Formats a volume in cubic meters according to player preferences.
	/// </summary>
	/// <param name="volumeUnit">[m³]</param>
	/// <returns>Formatted volume</returns>
	public static string FormatVolume(this VolumeUnit volumeUnit)
	{
		var volume = (DecimalNumber)volumeUnit;
		return $"{volume.ToString(decimals: 2)} m³";
	}

	//public static readonly string PressureUnit;

	/// <summary>
	/// Formats a pressure in pascal according to player preferences.
	/// </summary>
	/// <param name="pressureUnit">[N/m²]</param>
	/// <returns>Formatted pressure</returns>
	public static string FormatPressure(this PressureUnit pressureUnit)
	{
		var pressure = (DecimalNumber)pressureUnit;
		return $"{pressure.ToString(decimals: 2)} N/m²"; // PressureUnit
	}

	/// <summary>
	/// Formats a distance in meters according to player preferences.
	/// </summary>
	/// <param name="distance">[m]</param>
	/// <returns>Formatted distance</returns>
	public static string FormatDistance(this DistanceUnit distanceUnit)
	{
		var distance = (DecimalNumber)distanceUnit;
		return $"{distance.ToString(decimals: 2)} m";
	}

	/// <summary>
	/// Formats substance amount in moles according to player preferences.
	/// </summary>
	/// <param name="substanceAmount">[mol]</param>
	/// <returns>Formatted substance amount</returns>
	public static string FormatSubstanceAmount(this MolesUnit molesUnit)
	{
		var substanceAmount = (DecimalNumber)molesUnit;
		return $"{substanceAmount.ToString(decimals: 2)} mol";
	}

	/// <summary>
	/// Formats density in kilograms per cubic meter according to player preferences.
	/// </summary>
	/// <param name="density">[kg/m³]</param>
	/// <returns>Formatted density</returns>
	public static string FormatDensity(this DensityUnit densityUnit)
	{
		var density = (DecimalNumber)densityUnit;
		return $"{density.ToString(decimals: 2)} kg/m³";
	}

	/// <summary>
	/// Formats specific heat capacity in joules per kilogram kelvin according to player preferences.
	/// </summary>
	/// <param name="specificHeatCapacity">[J/kg*K]</param>
	/// <returns>Formatted specific heat capacity</returns>
	public static string FormatSpecificHeatCapacity(this SpecificHeatCapacityUnit specificHeatCapacityUnit)
	{
		var specificHeatCapacity = (DecimalNumber)specificHeatCapacityUnit;

		specificHeatCapacity.FormatStandardPrefix(out var prefix, out var scaledSpecificHeatCapacity);

		return $"{scaledSpecificHeatCapacity.ToString(decimals: 2)} {prefix}J/(kg*K)";
	}

	/// <summary>
	/// -273.15 ⁰C = absolute zero = 0 K
	/// </summary>
	public static readonly DecimalNumber AbsoluteZeroInCelcius = -273.15;

	/// <summary>
	/// Formats temperature in kelvin according to player preferences.
	/// </summary>
	/// <param name="temperature">[K]</param>
	/// <returns>Formatted temperature</returns>
	public static string FormatTemperature(this TemperatureUnit temperatureUnit)
	{
		var temperature = (DecimalNumber)temperatureUnit;

		if (useCelcius)
		{
			return $"{(temperature + AbsoluteZeroInCelcius).ToString(decimals: 2)} °C";
		}
		else
		{
			return $"{temperature.ToString(decimals: 2)} K";
		}
	}

	/// <summary>
	/// Formats energy in joules according to player preferences.
	/// </summary>
	/// <param name="energy">[J]</param>
	/// <returns>Formatted energy</returns>
	public static string FormatEnergy(this EnergyUnit energyUnit)
	{
		var energy = (DecimalNumber)energyUnit;

		energy.FormatStandardPrefix(out var prefix, out var scaledEnergy);

		return $"{scaledEnergy.ToString(decimals: 2)} {prefix}J";
	}

	public static string FormatMolarEnergy(this MolarEnergyUnit molarEnergyUnit)
	{
		var molarEnergy = (DecimalNumber)molarEnergyUnit;

		molarEnergy.FormatStandardPrefix(out var prefix, out var scaledMolarEnergy);

		return $"{scaledMolarEnergy.ToString(decimals: 2)} {prefix}J/mol";
	}

	/// <summary>
	/// Formats a value between zero and one as a percentage according to player preferences.
	/// Values greater than one or smaller than zero produce a percentage larger than 100 % or smaller than 0 %.
	/// </summary>
	/// <param name="value">0 <= value <= 1</param>
	/// <returns>Formatted percentage</returns>
	public static string FormatPercentage(this DecimalNumber value)
	{
		return $"{Math.Round((value * 100).ToDecimal(), 1)} %";
	}

	/// <summary>
	/// Formats a value between zero and one as a percentage according to player preferences.
	/// Values greater than one or smaller than zero produce a percentage larger than 100 % or smaller than 0 %.
	/// </summary>
	/// <param name="value">0 <= value <= 1</param>
	/// <returns>Formatted percentage</returns>
	public static string FormatPercentage<T>(this Portion<T> value)
		where T :
			IUnit<T>,
			IPortionable<T>,
			IIntervalSupport<T>
	{
		return $"{Math.Round(((DecimalNumber)value * 100).ToDecimal(), 1)} %";
	}

	public static void FormatStandardPrefix(this DecimalNumber unscaledValue, out string prefix, out DecimalNumber scaledValue)
	{
		if (unscaledValue > DecimalNumber.Giga)
		{
			prefix = "G";
			scaledValue = unscaledValue / DecimalNumber.Giga;
		}
		else if (unscaledValue > DecimalNumber.Mega)
		{
			prefix = "M";
			scaledValue = unscaledValue / DecimalNumber.Mega;
		}
		else if (unscaledValue > DecimalNumber.Kilo)
		{
			prefix = "k";
			scaledValue = unscaledValue / DecimalNumber.Kilo;
		}
		else
		{
			prefix = string.Empty;
			scaledValue = unscaledValue;
		}
	}
}
