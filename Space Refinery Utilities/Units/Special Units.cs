#if DEBUG
#define IncludeUnits
#endif



using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

#if IncludeUnits
/// <summary>
/// [k<typeparamref name="TUnit"/>]
/// </summary>
public struct Kilo<TUnit> : IUnit<Kilo<TUnit>>
	where TUnit : IUnit<TUnit>
{
	internal DecimalNumber value;

	public Kilo(DecimalNumber value)
	{
		this.value = value;
	}

	//public static MolarUnit operator *(_ExampleUnit grams, MolesUnit moles)
	//{
	//	return new(grams.value / moles.value);
	//}

	public static explicit operator DecimalNumber(Kilo<TUnit> unit) => unit.value;

	public static explicit operator Kilo<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Kilo<TUnit>(int value) => new(value);

	public static implicit operator Kilo<TUnit>(double value) => new(value);
}

// add Milli

/// <summary>
/// [<typeparamref name="TUnit"/>/s]
/// </summary>
public struct Rate<TUnit> : IUnit<Rate<TUnit>>
	where TUnit : IUnit<TUnit>
{
	internal DecimalNumber value;

	public Rate(DecimalNumber value)
	{
		this.value = value;
	}

	public static TUnit operator *(Rate<TUnit> rate, TimeUnit time)
	{
		return (TUnit)(rate.value * time.value);
	}

	public static TUnit operator /(TUnit unit, Rate<TUnit> rate)
	{
		return (TUnit)((DecimalNumber)unit / rate.value);
	}

	public static explicit operator DecimalNumber(Rate<TUnit> unit) => unit.value;

	public static explicit operator Rate<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Rate<TUnit>(int value) => new(value);

	public static implicit operator Rate<TUnit>(double value) => new(value);
}
#endif