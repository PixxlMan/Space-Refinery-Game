#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

// add more of these

/// <summary>
/// 
/// </summary>
public struct AmperageUnit : IUnit<AmperageUnit>
{
	internal DecimalNumber value;

	public AmperageUnit(DecimalNumber value)
	{
		this.value = value;
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(AmperageUnit unit) => unit.value;

	public static explicit operator AmperageUnit(DecimalNumber value) => new(value);

	public static implicit operator AmperageUnit(int value) => new(value);

	public static implicit operator AmperageUnit(double value) => new(value);

	public static bool operator >(AmperageUnit a, AmperageUnit b) => a.value > b.value;

	public static bool operator <(AmperageUnit a, AmperageUnit b) => a.value < b.value;

	public static bool operator >=(AmperageUnit a, AmperageUnit b) => a.value >= b.value;

	public static bool operator <=(AmperageUnit a, AmperageUnit b) => a.value <= b.value;

	public static bool operator ==(AmperageUnit a, AmperageUnit b) => a.Equals(b);

	public static bool operator !=(AmperageUnit a, AmperageUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is AmperageUnit unit && Equals(unit);
	}

	public bool Equals(AmperageUnit other)
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