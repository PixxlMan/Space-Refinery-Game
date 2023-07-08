#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [m³]
/// </summary>
public struct VolumeUnit : IUnit<VolumeUnit>
{
	internal DecimalNumber value;

	public VolumeUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static explicit operator DecimalNumber(VolumeUnit unit) => unit.value;

	public static explicit operator VolumeUnit(DecimalNumber value) => new(value);
}
#endif