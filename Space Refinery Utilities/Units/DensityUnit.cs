#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [kg/m³]
/// </summary>
public struct DensityUnit : IUnit<DensityUnit>
{
	internal DecimalNumber value;

	public DensityUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static explicit operator DecimalNumber(DensityUnit unit) => unit.value;

	public static explicit operator DensityUnit(DecimalNumber value) => new(value);

	public static implicit operator DensityUnit(int value) => new(value);

	public static implicit operator DensityUnit(double value) => new(value);
}
#endif