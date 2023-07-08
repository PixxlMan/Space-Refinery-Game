#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [mol] or n
/// </summary>
public struct MolesUnit : IUnit<MolesUnit>
{
	internal DecimalNumber value;

	public MolesUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static MolarUnit operator /(GramUnit grams, MolesUnit moles)
	{
		return new(grams.value / moles.value);
	}

	public static explicit operator DecimalNumber(MolesUnit unit) => unit.value;

	public static explicit operator MolesUnit(DecimalNumber value) => new(value);
}

/// <summary>
/// [g/mol] or M
/// </summary>
public struct MolarUnit : IUnit<MolarUnit>
{
	internal DecimalNumber value;

	public MolarUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static MolesUnit operator *(GramUnit mass, MolarUnit molar)
	{
		return new(mass.value * molar.value);
	}

	public static explicit operator DecimalNumber(MolarUnit unit) => unit.value;

	public static explicit operator MolarUnit(DecimalNumber value) => new(value);
}
#endif