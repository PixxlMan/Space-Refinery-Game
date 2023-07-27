#if DEBUG
#define IncludeUnits
#endif


using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// Divide [s/X] by this to get [X/s] and vice versa.
/// </summary>
/// <remarks>
/// This type stores no data and has no state.
/// It exists merely to provide a type safe way of performing conversions between intervals and rates.
/// </remarks>
public struct IntervalRateConversionUnit
{
	public static IntervalRateConversionUnit Unit = default;

	internal static DecimalNumber IntervalRateConversion => 1;

	public static implicit operator DecimalNumber(IntervalRateConversionUnit unit) => IntervalRateConversion;
}

#if IncludeUnits
/// <summary>
/// [s]
/// </summary>
/// <remarks>
/// Time, in seconds.
/// </remarks>
public struct TimeUnit :
	IUnit<TimeUnit>,
	IInterchangeable<TimeUnit, IntervalUnit>,
	IAdditionOperators<TimeUnit, TimeUnit, TimeUnit>,
	ISubtractionOperators<TimeUnit, TimeUnit, TimeUnit>
{
	internal DecimalNumber value;

	public TimeUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static implicit operator IntervalUnit(TimeUnit self)
	{
		return new(self.value);
	}

	public static TimeUnit operator +(TimeUnit a, TimeUnit b)
	{
		return new(a.value + b.value);
	}

	public static TimeUnit operator -(TimeUnit a, TimeUnit b)
	{
		return new(a.value - b.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(TimeUnit unit) => unit.value;

	public static explicit operator TimeUnit(DecimalNumber value) => new(value);

	public static implicit operator TimeUnit(int value) => new(value);

	public static implicit operator TimeUnit(double value) => new(value);

	public static bool operator >(TimeUnit a, TimeUnit b) => a.value > b.value;

	public static bool operator <(TimeUnit a, TimeUnit b) => a.value < b.value;

	public static bool operator >=(TimeUnit a, TimeUnit b) => a.value >= b.value;

	public static bool operator <=(TimeUnit a, TimeUnit b) => a.value <= b.value;

	public static bool operator ==(TimeUnit a, TimeUnit b) => a.Equals(b);

	public static bool operator !=(TimeUnit a, TimeUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is TimeUnit unit && Equals(unit);
	}

	public bool Equals(TimeUnit other)
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
/// [s]
/// </summary>
/// <remarks>
/// A time interval, in seconds.
/// <para/>
/// This type is interchangeable with <c>TimeUnit</c>.
/// </remarks>
public struct IntervalUnit : IUnit<IntervalUnit>, IInterchangeable<IntervalUnit, TimeUnit>
{
	internal DecimalNumber value;

	public IntervalUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static implicit operator TimeUnit(IntervalUnit self)
	{
		return new(self.value);
	}

	public static RateUnit operator /(IntervalRateConversionUnit intervalToRateUnit, IntervalUnit self)
	{
		return new(IntervalRateConversionUnit.IntervalRateConversion / self.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(IntervalUnit unit) => unit.value;

	public static explicit operator IntervalUnit(DecimalNumber value) => new(value);

	public static implicit operator IntervalUnit(int value) => new(value);

	public static implicit operator IntervalUnit(double value) => new(value);

	public static bool operator >(IntervalUnit a, IntervalUnit b) => a.value > b.value;

	public static bool operator <(IntervalUnit a, IntervalUnit b) => a.value < b.value;

	public static bool operator >=(IntervalUnit a, IntervalUnit b) => a.value >= b.value;

	public static bool operator <=(IntervalUnit a, IntervalUnit b) => a.value <= b.value;

	public static bool operator ==(IntervalUnit a, IntervalUnit b) => a.Equals(b);

	public static bool operator !=(IntervalUnit a, IntervalUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is IntervalUnit unit && Equals(unit);
	}

	public bool Equals(IntervalUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

public struct RateUnit : IUnit<RateUnit>
{
	internal DecimalNumber value;

	public RateUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static IntervalUnit operator /(IntervalRateConversionUnit intervalToRateUnit, RateUnit self)
	{
		return new(IntervalRateConversionUnit.IntervalRateConversion / self.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(RateUnit unit) => unit.value;

	public static explicit operator RateUnit(DecimalNumber value) => new(value);

	public static implicit operator RateUnit(int value) => new(value);

	public static implicit operator RateUnit(double value) => new(value);

	public static bool operator >(RateUnit a, RateUnit b) => a.value > b.value;

	public static bool operator <(RateUnit a, RateUnit b) => a.value < b.value;

	public static bool operator >=(RateUnit a, RateUnit b) => a.value >= b.value;

	public static bool operator <=(RateUnit a, RateUnit b) => a.value <= b.value;

	public static bool operator ==(RateUnit a, RateUnit b) => a.Equals(b);

	public static bool operator !=(RateUnit a, RateUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is RateUnit unit && Equals(unit);
	}

	public bool Equals(RateUnit other)
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
/// [s/<typeparamref name="TUnit"/>]
/// </summary>
/// <remarks>
/// Seconds per <typeparamref name="TUnit"/>, in seconds.
/// <para/>
/// This type is interchangeable with <c>Rate<TUnit></c>.
/// </remarks>
public struct Interval<TUnit> : IUnit<Interval<TUnit>>/*, IInterchangeable<IntervalUnit<TUnit>, Rate<TUnit>>*/ // Making it interchangeable would be cool, but would require it to still be present when not in Debug. Maybe that should be allowed for some types?
	where TUnit : IUnit<TUnit>
{
	internal DecimalNumber value;

	public Interval(DecimalNumber value)
	{
		this.value = value;
	}

	public static Rate<TUnit> operator /(IntervalRateConversionUnit intervalToRateUnit, Interval<TUnit> self)
	{
		return new(IntervalRateConversionUnit.IntervalRateConversion / self.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(Interval<TUnit> unit) => unit.value;

	public static explicit operator Interval<TUnit>(DecimalNumber value) => new(value);

	public static implicit operator Interval<TUnit>(int value) => new(value);

	public static implicit operator Interval<TUnit>(double value) => new(value);

	public static bool operator >(Interval<TUnit> a, Interval<TUnit> b) => a.value > b.value;

	public static bool operator <(Interval<TUnit> a, Interval<TUnit> b) => a.value < b.value;

	public static bool operator >=(Interval<TUnit> a, Interval<TUnit> b) => a.value >= b.value;

	public static bool operator <=(Interval<TUnit> a, Interval<TUnit> b) => a.value <= b.value;

	public static bool operator ==(Interval<TUnit> a, Interval<TUnit> b) => a.Equals(b);

	public static bool operator !=(Interval<TUnit> a, Interval<TUnit> b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is Interval<TUnit> unit && Equals(unit);
	}

	public bool Equals(Interval<TUnit> other)
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

	public static Interval<TUnit> operator /(IntervalRateConversionUnit intervalToRateUnit, Rate<TUnit> self)
	{
		return new(IntervalRateConversionUnit.IntervalRateConversion / self.value);
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