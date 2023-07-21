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

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(DensityUnit unit) => unit.value;

	public static explicit operator DensityUnit(DecimalNumber value) => new(value);

	public static implicit operator DensityUnit(int value) => new(value);

	public static implicit operator DensityUnit(double value) => new(value);

	public static bool operator >(DensityUnit a, DensityUnit b) => a.value > b.value;

	public static bool operator <(DensityUnit a, DensityUnit b) => a.value < b.value;

	public static bool operator >=(DensityUnit a, DensityUnit b) => a.value >= b.value;

	public static bool operator <=(DensityUnit a, DensityUnit b) => a.value <= b.value;

	public static bool operator ==(DensityUnit a, DensityUnit b) => a.Equals(b);

	public static bool operator !=(DensityUnit a, DensityUnit b) => !a.Equals(b);

	public override bool Equals(object? obj)
	{
		return obj is DensityUnit unit && Equals(unit);
	}

	public bool Equals(DensityUnit other)
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