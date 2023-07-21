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

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(Kilo<TUnit> unit) => unit.value;

	public static explicit operator Kilo<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Kilo<TUnit>(int value) => new(value);

	public static implicit operator Kilo<TUnit>(double value) => new(value);

	public static bool operator >(Kilo<TUnit> a, Kilo<TUnit> b) => a.value > b.value;

	public static bool operator <(Kilo<TUnit> a, Kilo<TUnit> b) => a.value < b.value;

	public static bool operator >=(Kilo<TUnit> a, Kilo<TUnit> b) => a.value >= b.value;

	public static bool operator <=(Kilo<TUnit> a, Kilo<TUnit> b) => a.value <= b.value;

	public static bool operator ==(Kilo<TUnit> a, Kilo<TUnit> b) => a.Equals(b);

	public static bool operator !=(Kilo<TUnit> a, Kilo<TUnit> b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is Kilo<TUnit> unit && Equals(unit);
	}

	public bool Equals(Kilo<TUnit> other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
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

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(Rate<TUnit> unit) => unit.value;

	public static explicit operator Rate<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Rate<TUnit>(int value) => new(value);

	public static implicit operator Rate<TUnit>(double value) => new(value);

	public static bool operator >(Rate<TUnit> a, Rate<TUnit> b) => a.value > b.value;

	public static bool operator <(Rate<TUnit> a, Rate<TUnit> b) => a.value < b.value;

	public static bool operator >=(Rate<TUnit> a, Rate<TUnit> b) => a.value >= b.value;

	public static bool operator <=(Rate<TUnit> a, Rate<TUnit> b) => a.value <= b.value;

	public static bool operator ==(Rate<TUnit> a, Rate<TUnit> b) => a.Equals(b);

	public static bool operator !=(Rate<TUnit> a, Rate<TUnit> b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is Rate<TUnit> unit && Equals(unit);
	}

	public bool Equals(Rate<TUnit> other)
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