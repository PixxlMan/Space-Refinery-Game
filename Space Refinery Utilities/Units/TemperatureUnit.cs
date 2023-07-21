#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [K]
/// </summary>
/// <remarks>
/// Temperature, in kelvin.
/// </remarks>
public struct TemperatureUnit : IUnit<TemperatureUnit>, IAdditionOperators<TemperatureUnit, TemperatureUnit, TemperatureUnit>
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
public struct SpecificHeatCapacityUnit : IUnit<SpecificHeatCapacityUnit>
{
	internal DecimalNumber value;

	public SpecificHeatCapacityUnit(DecimalNumber value)
	{
		this.value = value;
	}

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
public struct HeatCapacityUnit : IUnit<HeatCapacityUnit>
{
	internal DecimalNumber value;

	public HeatCapacityUnit(DecimalNumber value)
	{
		this.value = value;
	}

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