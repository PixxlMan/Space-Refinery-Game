﻿#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [J]
/// </summary>
public struct EnergyUnit :
	IUnit<EnergyUnit>,
	ISubtractionOperators<EnergyUnit, EnergyUnit, EnergyUnit>,
	IAdditionOperators<EnergyUnit, EnergyUnit, EnergyUnit>,
	IPortionable<EnergyUnit>,
	IIntervalSupport<EnergyUnit>
{
	internal DecimalNumber value;

	public EnergyUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [J] / [mol] => [J/mol]
	/// </summary>
	/// <param name="energyUnit">[J]</param>
	/// <param name="molesUnit">[mol]</param>
	/// <returns>[J/mol]</returns>
	public static MolarEnergyUnit operator /(EnergyUnit energyUnit, MolesUnit molesUnit)
	{
		return new(energyUnit.value / molesUnit.value);
	}
	
	/// <summary>
	/// [J] / [J/K] => [K]
	/// </summary>
	/// <param name="energyUnit">[J]</param>
	/// <param name="heatCapacityUnit">[J/K]</param>
	/// <returns>[K]</returns>
	public static TemperatureUnit operator /(EnergyUnit energyUnit, HeatCapacityUnit heatCapacityUnit)
	{
		return new(energyUnit.value / heatCapacityUnit.value);
	}

	public static EnergyUnit operator +(EnergyUnit left, EnergyUnit right)
	{
		return new(left.value + right.value);
	}

	public static EnergyUnit operator -(EnergyUnit left, EnergyUnit right)
	{
		return new(left.value -right.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(EnergyUnit unit) => unit.value;

	public static explicit operator EnergyUnit(DecimalNumber value) => new(value);

	public static implicit operator EnergyUnit(int value) => new(value);

	public static implicit operator EnergyUnit(double value) => new(value);

	public static bool operator >(EnergyUnit a, EnergyUnit b) => a.value > b.value;

	public static bool operator <(EnergyUnit a, EnergyUnit b) => a.value < b.value;

	public static bool operator >=(EnergyUnit a, EnergyUnit b) => a.value >= b.value;

	public static bool operator <=(EnergyUnit a, EnergyUnit b) => a.value <= b.value;

	public static bool operator ==(EnergyUnit a, EnergyUnit b) => a.Equals(b);

	public static bool operator !=(EnergyUnit a, EnergyUnit b) => !a.Equals(b);

	public static EnergyUnit operator -(EnergyUnit value)
	{
		return new(-value.value);
	}

	public static Portion<EnergyUnit> operator /(EnergyUnit left, EnergyUnit right)
	{
		return new(left.value / right.value);
	}

	public static EnergyUnit operator *(IntervalUnit interval, EnergyUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static EnergyUnit operator *(EnergyUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is EnergyUnit unit && Equals(unit);
	}

	public bool Equals(EnergyUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

// MolesEnergyUnit instead, since it isn't connected to molar?
/// <summary>
/// [J/mol]
/// </summary>
public struct MolarEnergyUnit :
	IUnit<MolarEnergyUnit>,
	IPortionable<MolarEnergyUnit>,
	IIntervalSupport<MolarEnergyUnit>
{
	internal DecimalNumber value;

	public MolarEnergyUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [J/mol] * [mol] => [J]
	/// </summary>
	/// <param name="molarEnergyUnit">[J/mol]</param>
	/// <param name="molesUnit">[mol]</param>
	/// <returns>[J]</returns>
	public static EnergyUnit operator *(MolarEnergyUnit molarEnergyUnit, MolesUnit molesUnit)
	{
		return new(molarEnergyUnit.value * molesUnit.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(MolarEnergyUnit unit) => unit.value;

	public static explicit operator MolarEnergyUnit(DecimalNumber value) => new(value);

	public static implicit operator MolarEnergyUnit(int value) => new(value);

	public static implicit operator MolarEnergyUnit(double value) => new(value);

	public static bool operator >(MolarEnergyUnit a, MolarEnergyUnit b) => a.value > b.value;

	public static bool operator <(MolarEnergyUnit a, MolarEnergyUnit b) => a.value < b.value;

	public static bool operator >=(MolarEnergyUnit a, MolarEnergyUnit b) => a.value >= b.value;

	public static bool operator <=(MolarEnergyUnit a, MolarEnergyUnit b) => a.value <= b.value;

	public static bool operator ==(MolarEnergyUnit a, MolarEnergyUnit b) => a.Equals(b);

	public static bool operator !=(MolarEnergyUnit a, MolarEnergyUnit b) => !a.Equals(b);

	public static MolarEnergyUnit operator -(MolarEnergyUnit value)
	{
		return new(-value.value);
	}

	public static Portion<MolarEnergyUnit> operator /(MolarEnergyUnit left, MolarEnergyUnit right)
	{
		return new(left.value / right.value);
	}

	public static MolarEnergyUnit operator *(IntervalUnit interval, MolarEnergyUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static MolarEnergyUnit operator *(MolarEnergyUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is MolarEnergyUnit unit && Equals(unit);
	}

	public bool Equals(MolarEnergyUnit other)
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