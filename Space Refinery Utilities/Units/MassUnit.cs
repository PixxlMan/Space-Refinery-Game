#if DEBUG
#define IncludeUnits
#endif


using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;

// Always include this, that way conversions to gram can be done outside of debug as well!
/// <summary>
/// Multiply [kg] by this to get [g].
/// </summary>
/// <remarks>
/// This type stores no data and has no state.
/// It exists merely to provide a type safe way of performing conversions between kilograms and grams.
/// </remarks>
public struct ToGramUnit
{
	public static ToGramUnit Unit = default;

	internal static DecimalNumber KilogramToGram => DecimalNumber.Milli;

	public static implicit operator DecimalNumber(ToGramUnit unit) => KilogramToGram;
}

/// <summary>
/// Multiply [g] by this to get [kg].
/// </summary>
/// <remarks>
/// This type stores no data and has no state.
/// It exists merely to provide a type safe way of performing conversions between kilograms and grams.
/// </remarks>
public struct ToKilogramUnit
{
	public static ToKilogramUnit Unit = default;

	internal static DecimalNumber GramToKilogram => DecimalNumber.Kilo;

	public static implicit operator DecimalNumber(ToKilogramUnit unit) => GramToKilogram;
}

#if IncludeUnits
/// <summary>
/// [kg]
/// </summary>
/// <remarks>
/// TODO add which conversions are possible here?
/// </remarks>
public struct MassUnit :
	IUnit<MassUnit>,
	IAdditionOperators<MassUnit, MassUnit, MassUnit>,
	ISubtractionOperators<MassUnit, MassUnit, MassUnit>,
	IPortionable<MassUnit>,
	IIntervalSupport<MassUnit>
{
	internal DecimalNumber value;

	public MassUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [kg] / [m³] => [kg/m³]
	/// </summary>
	/// <param name="mass">[kg]</param>
	/// <param name="volume">[m³]</param>
	/// <returns>[kg/m³]</returns>
	public static DensityUnit operator /(MassUnit mass, VolumeUnit volume)
	{
		return new(mass.value / volume.value);
	}

	/// <summary>
	/// [kg] / [kg/m³] => [m³]
	/// </summary>
	/// <param name="mass">[kg]</param>
	/// <param name="density">[kg/m³]</param>
	/// <returns>[m³]</returns>
	public static VolumeUnit operator /(MassUnit mass, DensityUnit density)
	{
		return new(mass.value / density.value);
	}

	public static GramUnit operator *(MassUnit mass, ToGramUnit unit)
	{
		return new(mass.value * ToGramUnit.KilogramToGram); // [kg] * (1 / [k]) => [g]
	}

	public static MassUnit operator +(MassUnit left, MassUnit right)
	{
		return new(left.value + right.value);
	}

	public static MassUnit operator -(MassUnit left, MassUnit right)
	{
		return new(left.value - right.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(MassUnit unit) => unit.value;

	public static explicit operator MassUnit(DecimalNumber value) => new(value);

	public static implicit operator MassUnit(int value) => new(value);

	public static implicit operator MassUnit(double value) => new(value);

	public static bool operator >(MassUnit a, MassUnit b) => a.value > b.value;

	public static bool operator <(MassUnit a, MassUnit b) => a.value < b.value;

	public static bool operator >=(MassUnit a, MassUnit b) => a.value >= b.value;

	public static bool operator <=(MassUnit a, MassUnit b) => a.value <= b.value;

	public static bool operator ==(MassUnit a, MassUnit b) => a.Equals(b);

	public static bool operator !=(MassUnit a, MassUnit b) => !a.Equals(b);

	public static MassUnit operator -(MassUnit value)
	{
		return new(-value.value);
	}

	public static Portion<MassUnit> operator /(MassUnit left, MassUnit right)
	{
		return new(left.value / right.value);
	}

	public static MassUnit operator *(IntervalUnit interval, MassUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static MassUnit operator *(MassUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is MassUnit unit && Equals(unit);
	}

	public bool Equals(MassUnit other)
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
/// [g]
/// </summary>
public struct GramUnit :
	IUnit<GramUnit>,
	IPortionable<GramUnit>,
	IIntervalSupport<GramUnit>
{
	internal DecimalNumber value;

	public GramUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static MassUnit operator *(GramUnit grams, ToKilogramUnit unit)
	{
		return new(grams.value * ToGramUnit.KilogramToGram); // [g] * [k] => [kg]
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(GramUnit unit) => unit.value;

	public static explicit operator GramUnit(DecimalNumber value) => new(value);

	public static implicit operator GramUnit(int value) => new(value);

	public static implicit operator GramUnit(double value) => new(value);

	public static bool operator >(GramUnit a, GramUnit b) => a.value > b.value;

	public static bool operator <(GramUnit a, GramUnit b) => a.value < b.value;

	public static bool operator >=(GramUnit a, GramUnit b) => a.value >= b.value;

	public static bool operator <=(GramUnit a, GramUnit b) => a.value <= b.value;

	public static bool operator ==(GramUnit a, GramUnit b) => a.Equals(b);

	public static bool operator !=(GramUnit a, GramUnit b) => !a.Equals(b);

	public static GramUnit operator -(GramUnit value)
	{
		return new(-value.value);
	}

	public static Portion<GramUnit> operator /(GramUnit left, GramUnit right)
	{
		return new(left.value / right.value);
	}

	public static GramUnit operator *(IntervalUnit interval, GramUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static GramUnit operator *(GramUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is GramUnit unit && Equals(unit);
	}

	public bool Equals(GramUnit other)
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