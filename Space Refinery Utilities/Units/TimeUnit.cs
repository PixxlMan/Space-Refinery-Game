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

	public static explicit operator DecimalNumber(TimeUnit unit) => unit.value;

	public static explicit operator TimeUnit(DecimalNumber value) => new(value);

	public static implicit operator TimeUnit(int value) => new(value);

	public static implicit operator TimeUnit(double value) => new(value);
}
#endif