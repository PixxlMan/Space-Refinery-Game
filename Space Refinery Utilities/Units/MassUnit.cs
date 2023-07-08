#if DEBUG
#define IncludeUnits
#endif

using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

// Always include this, that way conversions to gram can be done outside of debug as well!
public struct ToGramUnit
{
	public static DecimalNumber KilogramToGram => DecimalNumber.Milli;
}

public struct ToKilogramUnit
{
	public static DecimalNumber GramToKilogram => DecimalNumber.Kilo;
}

#if IncludeUnits
/// <summary>
/// [kg]
/// </summary>
public struct MassUnit : IUnit<MassUnit>
{
	internal DecimalNumber value;

	public MassUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static DensityUnit operator /(MassUnit mass, VolumeUnit volume)
	{
		return new(mass.value / volume.value);
	}

	public static GramUnit operator *(MassUnit mass, ToGramUnit unit)
	{
		return new (mass.value * ToGramUnit.KilogramToGram); // [kg] * (1 / [k]) => [g]
	}

	public static explicit operator DecimalNumber(MassUnit unit) => unit.value;

	public static explicit operator MassUnit(DecimalNumber value) => new(value);
}

/// <summary>
/// [g]
/// </summary>
public struct GramUnit : IUnit<GramUnit>
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

	public static explicit operator DecimalNumber(GramUnit unit) => unit.value;

	public static explicit operator GramUnit(DecimalNumber value) => new(value);
}
#endif