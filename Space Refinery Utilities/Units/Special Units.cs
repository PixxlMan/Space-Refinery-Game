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

public interface IPortionable<TSelf>
	where TSelf : IUnit<TSelf>, IPortionable<TSelf>
{
	public static virtual Portion<TSelf> operator /(TSelf left, TSelf right)
	{
		return new((TSelf)((DecimalNumber)left / (DecimalNumber)right));
	}
}

/// <summary>
/// [<typeparamref name="TUnit"/>/<typeparamref name="TUnit"/>]
/// </summary>
public struct Portion<TUnit> : IUnit<Portion<TUnit>>
	where TUnit : IUnit<TUnit>
{
	internal DecimalNumber value;

	public Portion(DecimalNumber value)
	{
		this.value = value;
	}
	
	public Portion(TUnit value)
	{
		this.value = (DecimalNumber)value;
	}

	// Copy this into any units that neeed support for portions, or use the interface.
	//public static Portion<TUnit> operator /(TUnit left, TUnit right)
	//{
	//	return new((TUnit)((DecimalNumber)left / (DecimalNumber)right));
	//}

	public static TUnit operator *(Portion<TUnit> portion, TUnit portioned)
	{
		return (TUnit)(portion.value * (DecimalNumber)portioned);
	}

	// Here we include both directions, should this be universally done?
	public static TUnit operator *(TUnit portioned, Portion<TUnit> portion) => portion * portioned;

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(Portion<TUnit> unit) => unit.value;

	public static explicit operator Portion<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Portion<TUnit>(int value) => new(value);

	public static implicit operator Portion<TUnit>(double value) => new(value);

	public static bool operator >(Portion<TUnit> a, Portion<TUnit> b) => a.value > b.value;

	public static bool operator <(Portion<TUnit> a, Portion<TUnit> b) => a.value < b.value;

	public static bool operator >=(Portion<TUnit> a, Portion<TUnit> b) => a.value >= b.value;

	public static bool operator <=(Portion<TUnit> a, Portion<TUnit> b) => a.value <= b.value;

	public static bool operator ==(Portion<TUnit> a, Portion<TUnit> b) => a.Equals(b);

	public static bool operator !=(Portion<TUnit> a, Portion<TUnit> b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is Portion<TUnit> unit && Equals(unit);
	}

	public bool Equals(Portion<TUnit> other)
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