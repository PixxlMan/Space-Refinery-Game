#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using System.Numerics;

namespace Space_Refinery_Engine.Units;

/// <summary>
/// [K]
/// </summary>
/// <remarks>
/// Temperature, in kelvin.
/// </remarks>
public struct TemperatureUnit :
	IUnit<TemperatureUnit>,
	IAdditionOperators<TemperatureUnit, TemperatureUnit, TemperatureUnit>,
	IPortionable<TemperatureUnit>,
	IIntervalSupport<TemperatureUnit>
{
	internal DecimalNumber value;

	public TemperatureUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static TemperatureUnit operator +(TemperatureUnit a, TemperatureUnit b)
	{
		return new(a.value + b.value);
	}

	public static TemperatureUnit operator -(TemperatureUnit a, TemperatureUnit b)
	{
		return new(a.value - b.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(TemperatureUnit unit) => unit.value;

	public static explicit operator TemperatureUnit(DecimalNumber value) => new(value);

	public static implicit operator TemperatureUnit(int value) => new(value);

	public static implicit operator TemperatureUnit(double value) => new(value);

	public static bool operator >(TemperatureUnit a, TemperatureUnit b) => a.value > b.value;

	public static bool operator <(TemperatureUnit a, TemperatureUnit b) => a.value < b.value;

	public static bool operator >=(TemperatureUnit a, TemperatureUnit b) => a.value >= b.value;

	public static bool operator <=(TemperatureUnit a, TemperatureUnit b) => a.value <= b.value;

	public static bool operator ==(TemperatureUnit a, TemperatureUnit b) => a.Equals(b);

	public static bool operator !=(TemperatureUnit a, TemperatureUnit b) => !a.Equals(b);

	public static TemperatureUnit operator -(TemperatureUnit value)
	{
		return new(-value.value);
	}

	public static Portion<TemperatureUnit> operator /(TemperatureUnit left, TemperatureUnit right)
	{
		return new(left.value / right.value);
	}

	public static TemperatureUnit operator *(IntervalUnit interval, TemperatureUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static TemperatureUnit operator *(TemperatureUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is TemperatureUnit unit && Equals(unit);
	}

	public bool Equals(TemperatureUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

/// <summary>
/// [J/(K*kg)]
/// </summary>
/// <remarks>
/// Specific heat capacity, the energy necessary to heat one kilogram of a given substance by 1 K.
/// </remarks>
public struct SpecificHeatCapacityUnit :
	IUnit<SpecificHeatCapacityUnit>,
	IPortionable<SpecificHeatCapacityUnit>,
	IIntervalSupport<SpecificHeatCapacityUnit>
{
	internal DecimalNumber value;

	public SpecificHeatCapacityUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [J/(K*kg)] * [kg] => [J/K]
	/// </summary>
	/// <param name="specificHeatCapacityUnit">[J/(K*kg)]</param>
	/// <param name="massUnit">[kg]</param>
	/// <returns>[J/K]</returns>
	public static HeatCapacityUnit operator *(SpecificHeatCapacityUnit specificHeatCapacityUnit, MassUnit massUnit)
		=> new(specificHeatCapacityUnit.value * massUnit.value);

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(SpecificHeatCapacityUnit unit) => unit.value;

	public static explicit operator SpecificHeatCapacityUnit(DecimalNumber value) => new(value);

	public static implicit operator SpecificHeatCapacityUnit(int value) => new(value);

	public static implicit operator SpecificHeatCapacityUnit(double value) => new(value);

	public static bool operator >(SpecificHeatCapacityUnit a, SpecificHeatCapacityUnit b) => a.value > b.value;

	public static bool operator <(SpecificHeatCapacityUnit a, SpecificHeatCapacityUnit b) => a.value < b.value;

	public static bool operator >=(SpecificHeatCapacityUnit a, SpecificHeatCapacityUnit b) => a.value >= b.value;

	public static bool operator <=(SpecificHeatCapacityUnit a, SpecificHeatCapacityUnit b) => a.value <= b.value;

	public static bool operator ==(SpecificHeatCapacityUnit a, SpecificHeatCapacityUnit b) => a.Equals(b);

	public static bool operator !=(SpecificHeatCapacityUnit a, SpecificHeatCapacityUnit b) => !a.Equals(b);

	public static SpecificHeatCapacityUnit operator -(SpecificHeatCapacityUnit value)
	{
		return new(-value.value);
	}

	public static Portion<SpecificHeatCapacityUnit> operator /(SpecificHeatCapacityUnit left, SpecificHeatCapacityUnit right)
	{
		return new(left.value / right.value);
	}

	public static SpecificHeatCapacityUnit operator *(IntervalUnit interval, SpecificHeatCapacityUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static SpecificHeatCapacityUnit operator *(SpecificHeatCapacityUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is SpecificHeatCapacityUnit unit && Equals(unit);
	}

	public bool Equals(SpecificHeatCapacityUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

/// <summary>
/// [J/K]
/// </summary>
/// <remarks>
/// Heat capacity, energy requited to heat a given mass of a substance.
/// </remarks>
public struct HeatCapacityUnit :
	IUnit<HeatCapacityUnit>,
	IPortionable<HeatCapacityUnit>,
	IIntervalSupport<HeatCapacityUnit>
{
	internal DecimalNumber value;

	public HeatCapacityUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [J/K] * [K] => [J]
	/// </summary>
	/// <param name="heatCapacityUnit">[J/K]</param>
	/// <param name="temperatureUnit">[K]</param>
	/// <returns>[J]</returns>
	public static EnergyUnit operator *(HeatCapacityUnit heatCapacityUnit, TemperatureUnit temperatureUnit)
		=> new(heatCapacityUnit.value * temperatureUnit.value);

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(HeatCapacityUnit unit) => unit.value;

	public static explicit operator HeatCapacityUnit(DecimalNumber value) => new(value);

	public static implicit operator HeatCapacityUnit(int value) => new(value);

	public static implicit operator HeatCapacityUnit(double value) => new(value);

	public static bool operator >(HeatCapacityUnit a, HeatCapacityUnit b) => a.value > b.value;

	public static bool operator <(HeatCapacityUnit a, HeatCapacityUnit b) => a.value < b.value;

	public static bool operator >=(HeatCapacityUnit a, HeatCapacityUnit b) => a.value >= b.value;

	public static bool operator <=(HeatCapacityUnit a, HeatCapacityUnit b) => a.value <= b.value;

	public static bool operator ==(HeatCapacityUnit a, HeatCapacityUnit b) => a.Equals(b);

	public static bool operator !=(HeatCapacityUnit a, HeatCapacityUnit b) => !a.Equals(b);

	public static HeatCapacityUnit operator -(HeatCapacityUnit value)
	{
		return new(-value.value);
	}

	public static Portion<HeatCapacityUnit> operator /(HeatCapacityUnit left, HeatCapacityUnit right)
	{
		return new(left.value / right.value);
	}

	public static HeatCapacityUnit operator *(IntervalUnit interval, HeatCapacityUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static HeatCapacityUnit operator *(HeatCapacityUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is HeatCapacityUnit unit && Equals(unit);
	}

	public bool Equals(HeatCapacityUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}
#endif