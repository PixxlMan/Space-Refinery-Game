#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

/// <summary>
/// 
/// </summary>
public struct _ExampleUnit :
	IUnit<_ExampleUnit>,
	IPortionable<_ExampleUnit>,
	IIntervalSupport<_ExampleUnit>
{
	internal DecimalNumber value;

	public _ExampleUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static MolarUnit operator /(_ExampleUnit grams, MolesUnit moles)
	{
		return new(grams.value / moles.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(_ExampleUnit unit) => unit.value;

	public static explicit operator _ExampleUnit(DecimalNumber value) => new(value);

	public static implicit operator _ExampleUnit(int value) => new(value);

	public static implicit operator _ExampleUnit(double value) => new(value);

	public static bool operator >(_ExampleUnit a, _ExampleUnit b) => a.value > b.value;

	public static bool operator <(_ExampleUnit a, _ExampleUnit b) => a.value < b.value;

	public static bool operator >=(_ExampleUnit a, _ExampleUnit b) => a.value >= b.value;

	public static bool operator <=(_ExampleUnit a, _ExampleUnit b) => a.value <= b.value;

	public static bool operator ==(_ExampleUnit a, _ExampleUnit b) => a.Equals(b);

	public static bool operator !=(_ExampleUnit a, _ExampleUnit b) => !a.Equals(b);

	public static _ExampleUnit operator -(_ExampleUnit value)
	{
		return new(-value.value);
	}

	public static Portion<_ExampleUnit> operator /(_ExampleUnit left, _ExampleUnit right)
	{
		return new(left.value / right.value);
	}

	public static _ExampleUnit operator *(IntervalUnit interval, _ExampleUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static _ExampleUnit operator *(_ExampleUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is _ExampleUnit unit && Equals(unit);
	}

	public bool Equals(_ExampleUnit other)
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