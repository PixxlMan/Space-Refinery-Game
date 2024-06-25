#if DEBUG
#define IncludeUnits
#endif


#if IncludeUnits
using System.Numerics;

namespace Space_Refinery_Engine.Units;

/// <summary>
/// [m³]
/// </summary>
public struct VolumeUnit :
	IUnit<VolumeUnit>,
	ISubtractionOperators<VolumeUnit, VolumeUnit, VolumeUnit>,
	IAdditionOperators<VolumeUnit, VolumeUnit, VolumeUnit>,
	IPortionable<VolumeUnit>,
	IIntervalSupport<VolumeUnit>
{
	internal DecimalNumber value;

	public VolumeUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [m³] * [kg/m³] => [kg]
	/// </summary>
	/// <param name="volumeUnit">[m³]</param>
	/// <param name="densityUnit">[kg/m³]</param>
	/// <returns>[kg]</returns>
	public static MassUnit operator *(VolumeUnit volumeUnit, DensityUnit densityUnit)
		=> new(volumeUnit.value * densityUnit.value);

	/// <summary>
	/// [kg/m³] * [m³] => [kg]
	/// </summary>
	/// <param name="densityUnit">[kg/m³]</param>
	/// <param name="volumeUnit">[m³]</param>
	/// <returns>[kg]</returns>
	public static MassUnit operator *(DensityUnit densityUnit, VolumeUnit volumeUnit)
		=> volumeUnit * densityUnit;

	public static VolumeUnit operator +(VolumeUnit left, VolumeUnit right)
		=> new(left.value + right.value);

	public static VolumeUnit operator -(VolumeUnit left, VolumeUnit right)
		=> new(left.value - right.value);

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

	public static VolumeUnit operator -(VolumeUnit value)
	{
		return new(-value.value);
	}

	public static Portion<VolumeUnit> operator /(VolumeUnit left, VolumeUnit right)
	{
		return new(left.value / right.value);
	}

	public static VolumeUnit operator *(IntervalUnit interval, VolumeUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static VolumeUnit operator *(VolumeUnit unit, IntervalUnit interval)
		=> interval * unit;

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
}
#endif