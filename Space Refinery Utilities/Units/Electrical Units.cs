#if DEBUG
#define IncludeUnits
#endif

using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [C/mol]
/// </summary>
/// <remarks>
/// This type stores no data and has no state.
/// It exists merely to provide a type safe way of performing calculations with the faraday constant.
/// </remarks>
public struct FaradayConstantUnit
{
	/// <summary>
	/// [C/mol]
	/// </summary>
	public static FaradayConstantUnit Unit = default;

	/// <summary>
	/// [C/mol]
	/// </summary>
	internal static DecimalNumber FaradayConstantValue => 96485;

	public static implicit operator DecimalNumber(FaradayConstantUnit unit) => Unit;

	/// <summary>
	/// [mol] * [C/mol] => [C]
	/// </summary>
	/// <param name="molesUnit">[mol]</param>
	/// <param name="faradayConstantUnit">[C/mol]</param>
	/// <returns>[C]</returns>
	public static CoulombUnit operator *(MolesUnit molesUnit, FaradayConstantUnit faradayConstantUnit)
	{
		return new(molesUnit.value * FaradayConstantValue);
	}
}

#if IncludeUnits

// add more of these and finish them

/// <summary>
/// [A] or [C/s]
/// </summary>
public struct AmperageUnit : IUnit<AmperageUnit>
{
	internal DecimalNumber value;

	public AmperageUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static implicit operator Rate<CoulombUnit>(AmperageUnit amperageUnit)
	{
		return new(amperageUnit.value);
	}

	public static implicit operator AmperageUnit(Rate<CoulombUnit> coulombRateUnit)
	{
		return new(coulombRateUnit.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(AmperageUnit unit) => unit.value;

	public static explicit operator AmperageUnit(DecimalNumber value) => new(value);

	public static implicit operator AmperageUnit(int value) => new(value);

	public static implicit operator AmperageUnit(double value) => new(value);

	public static bool operator >(AmperageUnit a, AmperageUnit b) => a.value > b.value;

	public static bool operator <(AmperageUnit a, AmperageUnit b) => a.value < b.value;

	public static bool operator >=(AmperageUnit a, AmperageUnit b) => a.value >= b.value;

	public static bool operator <=(AmperageUnit a, AmperageUnit b) => a.value <= b.value;

	public static bool operator ==(AmperageUnit a, AmperageUnit b) => a.Equals(b);

	public static bool operator !=(AmperageUnit a, AmperageUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is AmperageUnit unit && Equals(unit);
	}

	public bool Equals(AmperageUnit other)
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
/// [V] or [J/C]
/// </summary>
public struct VoltageUnit : IUnit<VoltageUnit>
{
	internal DecimalNumber value;

	public VoltageUnit(DecimalNumber value)
	{
		this.value = value;
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(VoltageUnit unit) => unit.value;

	public static explicit operator VoltageUnit(DecimalNumber value) => new(value);

	public static implicit operator VoltageUnit(int value) => new(value);

	public static implicit operator VoltageUnit(double value) => new(value);

	public static bool operator >(VoltageUnit a, VoltageUnit b) => a.value > b.value;

	public static bool operator <(VoltageUnit a, VoltageUnit b) => a.value < b.value;

	public static bool operator >=(VoltageUnit a, VoltageUnit b) => a.value >= b.value;

	public static bool operator <=(VoltageUnit a, VoltageUnit b) => a.value <= b.value;

	public static bool operator ==(VoltageUnit a, VoltageUnit b) => a.Equals(b);

	public static bool operator !=(VoltageUnit a, VoltageUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is VoltageUnit unit && Equals(unit);
	}

	public bool Equals(VoltageUnit other)
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
/// [C]
/// </summary>
public struct CoulombUnit : IUnit<CoulombUnit>
{
	internal DecimalNumber value;

	public CoulombUnit(DecimalNumber value)
	{
		this.value = value;
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(CoulombUnit unit) => unit.value;

	public static explicit operator CoulombUnit(DecimalNumber value) => new(value);

	public static implicit operator CoulombUnit(int value) => new(value);

	public static implicit operator CoulombUnit(double value) => new(value);

	public static bool operator >(CoulombUnit a, CoulombUnit b) => a.value > b.value;

	public static bool operator <(CoulombUnit a, CoulombUnit b) => a.value < b.value;

	public static bool operator >=(CoulombUnit a, CoulombUnit b) => a.value >= b.value;

	public static bool operator <=(CoulombUnit a, CoulombUnit b) => a.value <= b.value;

	public static bool operator ==(CoulombUnit a, CoulombUnit b) => a.Equals(b);

	public static bool operator !=(CoulombUnit a, CoulombUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is CoulombUnit unit && Equals(unit);
	}

	public bool Equals(CoulombUnit other)
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