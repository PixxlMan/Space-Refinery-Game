#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// [m³]
/// </summary>
public struct VolumeUnit : IUnit<VolumeUnit>, ISubtractionOperators<VolumeUnit, VolumeUnit, VolumeUnit>, IAdditionOperators<VolumeUnit, VolumeUnit, VolumeUnit>
{
	internal DecimalNumber value;

	public VolumeUnit(DecimalNumber value)
	{
		this.value = value;
	}

	public static explicit operator DecimalNumber(VolumeUnit unit) => unit.value;

	public static explicit operator VolumeUnit(DecimalNumber value) => new(value);

	public static implicit operator VolumeUnit(int value) => new(value);

	public static implicit operator VolumeUnit(double value) => new(value);

	public static VolumeUnit operator +(VolumeUnit left, VolumeUnit right)
		=> new(left.value + right.value);

	public static VolumeUnit operator -(VolumeUnit left, VolumeUnit right)
		=> new(left.value - right.value);
}
#endif