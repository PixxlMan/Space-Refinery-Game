#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;


namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [kg]
/// </summary>
public struct MassUnit
{
	internal DecimalNumber value;

	public MassUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static DensityUnit operator /(MassUnit mass, VolumeUnit volume)
	{
		return new(mass, volume);
	}
}

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

	public DensityUnit(MassUnit mass, VolumeUnit volume)
	{
		value = mass.value / volume.value;
	}
}

/// <summary>
/// [m³]
/// </summary>
public struct VolumeUnit
{
	internal DecimalNumber value;

	public VolumeUnit(DecimalNumber value)
	{
		this.value = value;
	}
}
#endif