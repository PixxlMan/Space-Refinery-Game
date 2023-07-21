#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;


/// <summary>
/// [mol] or n
/// </summary>
// Should this be called SubstanceAmountUnit?
public struct MolesUnit : IUnit<MolesUnit>, IMultiplyOperators<MolesUnit, MolesUnit, MolesUnit>, IMultiplyOperators<MolesUnit, int, MolesUnit>, IAdditionOperators<MolesUnit, MolesUnit, MolesUnit>, ISubtractionOperators<MolesUnit, MolesUnit, MolesUnit>
{
	internal DecimalNumber value;

	public MolesUnit(DecimalNumber value)
	{
		this.value = value;
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(MolesUnit unit) => unit.value;

	public static explicit operator MolesUnit(DecimalNumber value) => new(value);

	public static implicit operator MolesUnit(int value) => new(value);

	public static implicit operator MolesUnit(double value) => new(value);

	public static bool operator >(MolesUnit a, MolesUnit b) => a.value > b.value;

	public static bool operator <(MolesUnit a, MolesUnit b) => a.value < b.value;

	public static bool operator >=(MolesUnit a, MolesUnit b) => a.value >= b.value;

	public static bool operator <=(MolesUnit a, MolesUnit b) => a.value <= b.value;

	public static bool operator ==(MolesUnit a, MolesUnit b) => a.Equals(b);

	public static bool operator !=(MolesUnit a, MolesUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is MolesUnit unit && Equals(unit);
	}

	public bool Equals(MolesUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion

	public static MolarUnit operator /(GramUnit grams, MolesUnit moles)
	{
		return new(grams.value / moles.value);
	}

	public static MolesUnit operator *(MolesUnit left, MolesUnit right)
	{
		return new(left.value * right.value);
	}

	public static MolesUnit operator *(MolesUnit left, DecimalNumber right)
	{
		return new(left.value * right);
	}
	
	public static MolesUnit operator *(MolesUnit left, int right)
	{
		return new(left.value * right);
	}

	public static MolesUnit operator +(MolesUnit left, MolesUnit right)
	{
		return new(left.value + right.value);
	}

	public static MolesUnit operator -(MolesUnit left, MolesUnit right)
	{
		return new(left.value - right.value);
	}
}

/// <summary>
/// [g/mol] <!--or M-->
/// </summary>
public struct MolarUnit : IUnit<MolarUnit>
{
	internal DecimalNumber value;

	public MolarUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [g] / [g/mol] => [mol]
	/// </summary>
	/// <param name="molar">[g/mol]</param>
	/// <param name="gramMass">[g]</param>
	/// <returns>[mol]</returns>
	public static MolesUnit operator /(GramUnit gramMass, MolarUnit molar)
	{
		return new(gramMass.value / molar.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(MolarUnit unit) => unit.value;

	public static explicit operator MolarUnit(DecimalNumber value) => new(value);

	public static implicit operator MolarUnit(int value) => new(value);

	public static implicit operator MolarUnit(double value) => new(value);

	public static bool operator >(MolarUnit a, MolarUnit b) => a.value > b.value;

	public static bool operator <(MolarUnit a, MolarUnit b) => a.value < b.value;

	public static bool operator >=(MolarUnit a, MolarUnit b) => a.value >= b.value;

	public static bool operator <=(MolarUnit a, MolarUnit b) => a.value <= b.value;

	public static bool operator ==(MolarUnit a, MolarUnit b) => a.Equals(b);

	public static bool operator !=(MolarUnit a, MolarUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is MolarUnit unit && Equals(unit);
	}

	public bool Equals(MolarUnit other)
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