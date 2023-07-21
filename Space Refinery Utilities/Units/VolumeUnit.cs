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

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(VolumeUnit unit) => unit.value;

	public static explicit operator VolumeUnit(DecimalNumber value) => new(value);

	public static implicit operator VolumeUnit(int value) => new(value);

	public static implicit operator VolumeUnit(double value) => new(value);

	public static bool operator >(VolumeUnit a, VolumeUnit b) => a.value > b.value;

	public static bool operator <(VolumeUnit a, VolumeUnit b) => a.value < b.value;

	public static bool operator >=(VolumeUnit a, VolumeUnit b) => a.value >= b.value;

	public static bool operator <=(VolumeUnit a, VolumeUnit b) => a.value <= b.value;

	public static bool operator ==(VolumeUnit a, VolumeUnit b) => a.Equals(b);

	public static bool operator !=(VolumeUnit a, VolumeUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is VolumeUnit unit && Equals(unit);
	}

	public bool Equals(VolumeUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion

	public static VolumeUnit operator +(VolumeUnit left, VolumeUnit right)
		=> new(left.value + right.value);

	public static VolumeUnit operator -(VolumeUnit left, VolumeUnit right)
		=> new(left.value - right.value);
}
#endif