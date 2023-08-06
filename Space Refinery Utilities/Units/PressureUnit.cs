#if DEBUG
#define IncludeUnits
#endif


using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// g = 9.82
/// <para/>
/// Multiply [N] by this to get [kg].
/// Divide [kg] by this to get [N].
/// </summary>
/// <remarks>
/// This type stores no data and has no state. It always equals 9.82.
/// It exists merely to provide a type safe way of performing calculations using the gravitational acceleration.
/// </remarks>
public struct GravitationalAccelerationUnit
{
	public static GravitationalAccelerationUnit Unit = default;

	/// <summary>
	/// The gravitational constant is 9.82.
	/// </summary>
	/// <remarks>
	/// I will make absolutely no concessions ever to this ;p
	/// </remarks>
	internal static DecimalNumber GravitationalAcceleration => 9.82;

	public static implicit operator DecimalNumber(GravitationalAccelerationUnit unit) => GravitationalAcceleration;
}

#if IncludeUnits
/// <summary>
/// [m]
/// </summary>
public struct DistanceUnit :
	IUnit<DistanceUnit>,
	IPortionable<DistanceUnit>,
	IIntervalSupport<DistanceUnit>
{
	internal DecimalNumber value;

	public DistanceUnit(DecimalNumber value)
	{
		this.value = value;
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(DistanceUnit unit) => unit.value;

	public static explicit operator DistanceUnit(DecimalNumber value) => new(value);

	public static implicit operator DistanceUnit(int value) => new(value);

	public static implicit operator DistanceUnit(double value) => new(value);

	public static bool operator >(DistanceUnit a, DistanceUnit b) => a.value > b.value;

	public static bool operator <(DistanceUnit a, DistanceUnit b) => a.value < b.value;

	public static bool operator >=(DistanceUnit a, DistanceUnit b) => a.value >= b.value;

	public static bool operator <=(DistanceUnit a, DistanceUnit b) => a.value <= b.value;

	public static bool operator ==(DistanceUnit a, DistanceUnit b) => a.Equals(b);

	public static bool operator !=(DistanceUnit a, DistanceUnit b) => !a.Equals(b);

	public static DistanceUnit operator -(DistanceUnit value)
	{
		return new(-value.value);
	}

	public static Portion<DistanceUnit> operator /(DistanceUnit left, DistanceUnit right)
	{
		return new(left.value / right.value);
	}

	public static DistanceUnit operator *(IntervalUnit interval, DistanceUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static DistanceUnit operator *(DistanceUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is DistanceUnit unit && Equals(unit);
	}

	public bool Equals(DistanceUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

/// <summary>
/// [m²]
/// </summary>
public struct AreaUnit :
	IUnit<AreaUnit>,
	IPortionable<AreaUnit>,
	IIntervalSupport<AreaUnit>
{
	internal DecimalNumber value;

	public AreaUnit(DecimalNumber value)
	{
		this.value = value;
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(AreaUnit unit) => unit.value;

	public static explicit operator AreaUnit(DecimalNumber value) => new(value);

	public static implicit operator AreaUnit(int value) => new(value);

	public static implicit operator AreaUnit(double value) => new(value);

	public static bool operator >(AreaUnit a, AreaUnit b) => a.value > b.value;

	public static bool operator <(AreaUnit a, AreaUnit b) => a.value < b.value;

	public static bool operator >=(AreaUnit a, AreaUnit b) => a.value >= b.value;

	public static bool operator <=(AreaUnit a, AreaUnit b) => a.value <= b.value;

	public static bool operator ==(AreaUnit a, AreaUnit b) => a.Equals(b);

	public static bool operator !=(AreaUnit a, AreaUnit b) => !a.Equals(b);

	public static AreaUnit operator -(AreaUnit value)
	{
		return new(-value.value);
	}

	public static Portion<AreaUnit> operator /(AreaUnit left, AreaUnit right)
	{
		return new(left.value / right.value);
	}

	public static AreaUnit operator *(IntervalUnit interval, AreaUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static AreaUnit operator *(AreaUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is AreaUnit unit && Equals(unit);
	}

	public bool Equals(AreaUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

/// <summary>
/// [N]
/// </summary>
/// <remarks>
/// Force, in Newtons.
/// </remarks>
public struct ForceUnit :
	IUnit<ForceUnit>,
	IPortionable<ForceUnit>,
	IIntervalSupport<ForceUnit>
{
	internal DecimalNumber value;

	public ForceUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [N] * G => [kg]
	/// </summary>
	/// <param name="forceUnit">[N]</param>
	/// <param name="gravitationalAccelerationUnit">g</param>
	/// <returns></returns>
	public static MassUnit operator *(ForceUnit forceUnit, GravitationalAccelerationUnit gravitationalAccelerationUnit)
	{
		return new(forceUnit.value * GravitationalAccelerationUnit.Unit);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(ForceUnit unit) => unit.value;

	public static explicit operator ForceUnit(DecimalNumber value) => new(value);

	public static implicit operator ForceUnit(int value) => new(value);

	public static implicit operator ForceUnit(double value) => new(value);

	public static bool operator >(ForceUnit a, ForceUnit b) => a.value > b.value;

	public static bool operator <(ForceUnit a, ForceUnit b) => a.value < b.value;

	public static bool operator >=(ForceUnit a, ForceUnit b) => a.value >= b.value;

	public static bool operator <=(ForceUnit a, ForceUnit b) => a.value <= b.value;

	public static bool operator ==(ForceUnit a, ForceUnit b) => a.Equals(b);

	public static bool operator !=(ForceUnit a, ForceUnit b) => !a.Equals(b);

	public static ForceUnit operator -(ForceUnit value)
	{
		return new(-value.value);
	}

	public static Portion<ForceUnit> operator /(ForceUnit left, ForceUnit right)
	{
		return new(left.value / right.value);
	}

	public static ForceUnit operator *(IntervalUnit interval, ForceUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static ForceUnit operator *(ForceUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is ForceUnit unit && Equals(unit);
	}

	public bool Equals(ForceUnit other)
	{
		return value.Equals(other.value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	#endregion
}

/// <summary>
/// [N/m²]
/// </summary>
/// <remarks>
/// Pressure, in Pascals [Pa], or Newtons per meter squared [N/m²].
/// </remarks>
public struct PressureUnit :
	IUnit<PressureUnit>,
	IPortionable<PressureUnit>,
	IIntervalSupport<PressureUnit>
{
	internal DecimalNumber value;

	public PressureUnit(DecimalNumber value)
	{
		this.value = value;
	}

	/// <summary>
	/// [N/m²] * [m²] => [N]
	/// </summary>
	/// <param name="pressureUnit">[N/m²]</param>
	/// <param name="areaUnit">[m²]</param>
	/// <returns>[N]</returns>
	public static ForceUnit operator *(PressureUnit pressureUnit, AreaUnit areaUnit)
	{
		return new(pressureUnit.value * areaUnit.value);
	}

	#region Operators and boilerplate

	public static explicit operator DecimalNumber(PressureUnit unit) => unit.value;

	public static explicit operator PressureUnit(DecimalNumber value) => new(value);

	public static implicit operator PressureUnit(int value) => new(value);

	public static implicit operator PressureUnit(double value) => new(value);

	public static bool operator >(PressureUnit a, PressureUnit b) => a.value > b.value;

	public static bool operator <(PressureUnit a, PressureUnit b) => a.value < b.value;

	public static bool operator >=(PressureUnit a, PressureUnit b) => a.value >= b.value;

	public static bool operator <=(PressureUnit a, PressureUnit b) => a.value <= b.value;

	public static bool operator ==(PressureUnit a, PressureUnit b) => a.Equals(b);

	public static bool operator !=(PressureUnit a, PressureUnit b) => !a.Equals(b);

	public static PressureUnit operator -(PressureUnit value)
	{
		return new(-value.value);
	}

	public static Portion<PressureUnit> operator /(PressureUnit left, PressureUnit right)
	{
		return new(left.value / right.value);
	}

	public static PressureUnit operator *(IntervalUnit interval, PressureUnit unit)
	{
		return new(interval.value * unit.value);
	}

	public static PressureUnit operator *(PressureUnit unit, IntervalUnit interval)
		=> interval * unit;

	public override bool Equals(object? obj)
	{
		return obj is PressureUnit unit && Equals(unit);
	}

	public bool Equals(PressureUnit other)
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