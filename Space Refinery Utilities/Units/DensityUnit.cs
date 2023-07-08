#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [kg/m³]
/// </summary>
public struct DensityUnit
{
	internal DecimalNumber value;

	public DensityUnit(DecimalNumber value)
	{
		this.value = value;
	}
}
#endif