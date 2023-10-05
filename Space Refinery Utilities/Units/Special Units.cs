#if DEBUG
#define IncludeUnits
#endif

using System.Numerics;

namespace Space_Refinery_Utilities.Units;

#if IncludeUnits
/// <summary>
/// [k<typeparamref name="TUnit"/>]
/// </summary>
public struct Kilo<TUnit> :
	IUnit<Kilo<TUnit>>,
	IPortionable<Kilo<TUnit>>,
	IIntervalSupport<Kilo<TUnit>>
	where TUnit :
		IUnit<TUnit>
{
	internal DecimalNumber value;

	public Kilo(DecimalNumber value)
	{
		this.value = value;
	}

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

	public static Kilo<TUnit> operator -(Kilo<TUnit> value)
	{
		return new(-value.value);
	}

	public static Portion<Kilo<TUnit>> operator /(Kilo<TUnit> left, Kilo<TUnit> right)
	{
		return new(left.value / right.value);
	}

	public static Kilo<TUnit> operator *(IntervalUnit interval, Kilo<TUnit> unit)
	{
		return new(interval.value * unit.value);
	}

	public static Kilo<TUnit> operator *(Kilo<TUnit> unit, IntervalUnit interval)
		=> interval * unit;

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
	where TSelf :
		IUnit<TSelf>,
		IPortionable<TSelf>//,
		//IIntervalSupport<TSelf>
{
	public static abstract Portion<TSelf> operator /(TSelf left, TSelf right);
}

// Create non-generic PortionUnit?

/// <summary>
/// [<typeparamref name="TUnit"/>/<typeparamref name="TUnit"/>]
/// </summary>
public struct Portion<TUnit> :
	IUnit<Portion<TUnit>>,
	IAdditionOperators<Portion<TUnit>, Portion<TUnit>, Portion<TUnit>>,
	ISubtractionOperators<Portion<TUnit>, Portion<TUnit>, Portion<TUnit>>,
	IPortionable<Portion<TUnit>>,
	IIntervalSupport<Portion<TUnit>>
	where TUnit :
		IUnit<TUnit>
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

	public static Portion<TUnit> operator +(Portion<TUnit> left, Portion<TUnit> right)
	{
		return new(left.value + right.value);
	}

	public static Portion<TUnit> operator -(Portion<TUnit> left, Portion<TUnit> right)
	{
		return new(left.value - right.value);
	}

	public static TUnit operator *(Portion<TUnit> portion, TUnit portioned)
	{
		return (TUnit)(portion.value * (DecimalNumber)portioned);
	}

	// Here we include both directions, should this be universally done?
	public static TUnit operator *(TUnit portioned, Portion<TUnit> portion) => portion * portioned;

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(Portion<TUnit> unit) => unit.value;

	public static explicit operator Portion<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Portion<TUnit>(int value) => new((DecimalNumber)value);

	public static implicit operator Portion<TUnit>(double value) => new((DecimalNumber)value);

	public static bool operator >(Portion<TUnit> a, Portion<TUnit> b) => a.value > b.value;

	public static bool operator <(Portion<TUnit> a, Portion<TUnit> b) => a.value < b.value;

	public static bool operator >=(Portion<TUnit> a, Portion<TUnit> b) => a.value >= b.value;

	public static bool operator <=(Portion<TUnit> a, Portion<TUnit> b) => a.value <= b.value;

	public static bool operator ==(Portion<TUnit> a, Portion<TUnit> b) => a.Equals(b);

	public static bool operator !=(Portion<TUnit> a, Portion<TUnit> b) => !a.Equals(b);

	public static Portion<TUnit> operator -(Portion<TUnit> value)
	{
		return new(-value.value);
	}

	public static Portion<Portion<TUnit>> operator /(Portion<TUnit> left, Portion<TUnit> right)
	{
		return new(left.value / right.value);
	}

	public static Portion<TUnit> operator *(IntervalUnit interval, Portion<TUnit> unit)
	{
		return new(interval.value * unit.value);
	}

	public static Portion<TUnit> operator *(Portion<TUnit> unit, IntervalUnit interval)
		=> interval * unit;

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