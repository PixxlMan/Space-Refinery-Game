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

	public static explicit operator DecimalNumber(AmperageUnit unit) => unit.value;

	public static explicit operator AmperageUnit(DecimalNumber value) => new(value);

	public static implicit operator AmperageUnit(int value) => new(value);

	public static implicit operator AmperageUnit(double value) => new(value);
}
#endif