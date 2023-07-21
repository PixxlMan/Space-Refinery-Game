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
public struct EnergyUnit : IUnit<EnergyUnit>,
	ISubtractionOperators<EnergyUnit, EnergyUnit, EnergyUnit>,
	IAdditionOperators<EnergyUnit, EnergyUnit, EnergyUnit>
{
	internal DecimalNumber value;

	public EnergyUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static MolarEnergyUnit operator /(EnergyUnit energyUnit, MolesUnit molesUnit)
	{
		return new(energyUnit.value / molesUnit.value);
	}

	/// <summary>
	/// [J] / [J/kg] => [kg]
	/// </summary>
	/// <param name="energyUnit">[J]</param>
	/// <param name="molarEnergyUnit">[J/mol]</param>
	/// <returns></returns>
	public static MolarEnergyUnit operator /(EnergyUnit energyUnit, MolarEnergyUnit molarEnergyUnit)
	{
		return new(energyUnit.value / molarEnergyUnit.value);
	}

	public static EnergyUnit operator +(EnergyUnit left, EnergyUnit right)
	{
		return new(left.value + right.value);
	}

	public static EnergyUnit operator -(EnergyUnit left, EnergyUnit right)
	{
		return new(left.value -right.value);
	}

	public static explicit operator DecimalNumber(EnergyUnit unit) => unit.value;

	public static explicit operator EnergyUnit(DecimalNumber value) => new(value);

	public static implicit operator EnergyUnit(int value) => new(value);

	public static implicit operator EnergyUnit(double value) => new(value);
}

// MolesEnergyUnit instead, since it isn't connected to molar?
/// <summary>
/// [J/mol]
/// </summary>
public struct MolarEnergyUnit : IUnit<MolarEnergyUnit>
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

	public static explicit operator DecimalNumber(MolarEnergyUnit unit) => unit.value;

	public static explicit operator MolarEnergyUnit(DecimalNumber value) => new(value);

	public static implicit operator MolarEnergyUnit(int value) => new(value);

	public static implicit operator MolarEnergyUnit(double value) => new(value);
}
#endif