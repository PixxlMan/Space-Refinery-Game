#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [s]
/// </summary>
/// <remarks>
/// Time, in seconds.
/// </remarks>
public struct TimeUnit : IUnit<TimeUnit>
{
	internal DecimalNumber value;

	public TimeUnit(DecimalNumber value)
	{
		this.value = value;
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
#endif