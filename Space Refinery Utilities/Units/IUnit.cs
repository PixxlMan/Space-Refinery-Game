using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// The purpose of Units is to achieve static type safety in physical and chemical calculations, with no runtime performance loss because units are removed and replaced by simple DecimalNumbers at runtime.
/// </summary>
public interface IUnit<TSelf> : IEquatable<TSelf>, IComparisonOperators<TSelf, TSelf, bool>
	where TSelf : IUnit<TSelf>
{
	public static abstract explicit operator DecimalNumber(TSelf unit);

	public static abstract explicit operator TSelf(DecimalNumber value);

	public static abstract implicit operator TSelf(int value);

	public static abstract implicit operator TSelf(double value);

	public static abstract bool operator ==(TSelf a, TSelf b);

	public static abstract bool operator !=(TSelf a, TSelf b);
}


//using FixedPrecision;
//using Space_Refinery_Game;

//namespace Space_Refinery_Utilities.Units;

///// <summary>
///// The purpose of Units is to achieve static type safety in physical and chemical calculations, with no runtime performance loss because units are removed and replaced by simple DecimalNumbers at runtime.
///// </summary>
//public partial interface IUnit<TSelf> : IFixedPrecisionNumeral<DecimalNumber>
//	where TSelf : IUnit<TSelf>
//{
//	internal abstract DecimalNumber DecimalNumber { get; }

//	public static byte Precision => DecimalNumber.Precision;

//	public static DecimalNumber MaxValue => DecimalNumber.MaxValue;

//	public static DecimalNumber SmallestIncrement => DecimalNumber.SmallestIncrement;

//	public static DecimalNumber One => DecimalNumber.One;

//	public static DecimalNumber Zero => DecimalNumber.Zero;

//	public static DecimalNumber PI => DecimalNumber.PI;

//	public static DecimalNumber DegreesToRadians => DecimalNumber.DegreesToRadians;

//	public static DecimalNumber RadiansToDegrees => DecimalNumber.RadiansToDegrees;

//	public static abstract explicit operator DecimalNumber(TSelf unit);

//	public static abstract explicit operator TSelf(DecimalNumber value);

//	public static abstract implicit operator TSelf(int value);

//	public static virtual implicit operator TSelf(short value)
//	{
//		return new(value);
//	}

//	public static virtual implicit operator TSelf(int value)
//	{
//		return new(value);
//	}

//	public static implicit operator TSelf(long value)
//	{
//		return new(value);
//	}

//	public static explicit operator DecimalNumber(decimal value)
//	{
//		return (DecimalNumber)value;
//	}

//	public static implicit operator DecimalNumber(double value)
//	{
//		return new(value);
//	}

//	public static explicit operator DecimalNumber(float value)
//	{
//		return (DecimalNumber)value;
//	}

//	public static explicit operator short(DecimalNumber value)
//	{
//		return value.ToInt16();
//	}

//	public static explicit operator int(DecimalNumber value)
//	{
//		return value.ToInt32();
//	}

//	public static explicit operator long(DecimalNumber value)
//	{
//		return value.ToInt64();
//	}

//	public static explicit operator decimal(DecimalNumber value)
//	{
//		return value.ToDecimal();
//	}

//	public static explicit operator double(DecimalNumber value)
//	{
//		return value.ToDouble();
//	}

//	public static explicit operator float(DecimalNumber value)
//	{
//		return value.ToFloat();
//	}

//	public static DecimalNumber FromFloat(float value)
//	{
//		return DecimalNumber.FromFloat(value);
//	}

//	public static DecimalNumber FromDouble(double value)
//	{
//		return DecimalNumber.FromDouble(value);
//	}

//	public static DecimalNumber FromDecimal(decimal value)
//	{
//		return DecimalNumber.FromDecimal(value);
//	}

//	public static DecimalNumber FromInt16(short value)
//	{
//		return DecimalNumber.FromInt16(value);
//	}

//	public static DecimalNumber FromInt32(int value)
//	{
//		return DecimalNumber.FromInt32(value);
//	}

//	public static DecimalNumber FromInt64(long value)
//	{
//		return DecimalNumber.FromInt64(value);
//	}

//	public static DecimalNumber Parse(string value)
//	{
//		return DecimalNumber.Parse(value);
//	}

//	public static bool TryParse(string value, out DecimalNumber result)
//	{
//		return DecimalNumber.TryParse(value, out result);
//	}

//	public float ToFloat()
//	{
//		return DecimalNumber.ToFloat();
//	}

//	public double ToDouble()
//	{
//		return DecimalNumber.ToDouble();
//	}

//	public decimal ToDecimal()
//	{
//		return DecimalNumber.ToDecimal();
//	}

//	public short ToInt16()
//	{
//		return DecimalNumber.ToInt16();
//	}

//	public int ToInt32()
//	{
//		return DecimalNumber.ToInt32();
//	}

//	public long ToInt64()
//	{
//		return DecimalNumber.ToInt64();
//	}

//	public float ToFloatRelativeToOrigin(DecimalNumber origin)
//	{
//		return DecimalNumber.ToFloatRelativeToOrigin(origin);
//	}

//	public double ToDoubleRelativeToOrigin(DecimalNumber origin)
//	{
//		return DecimalNumber.ToDoubleRelativeToOrigin(origin);
//	}

//	public static float ToFloat(DecimalNumber a)
//	{
//		return DecimalNumber.ToFloat(a);
//	}

//	public static double ToDouble(DecimalNumber a)
//	{
//		return DecimalNumber.ToDouble(a);
//	}

//	public static decimal ToDecimal(DecimalNumber a)
//	{
//		return DecimalNumber.ToDecimal(a);
//	}

//	public static short ToInt16(DecimalNumber a)
//	{
//		return DecimalNumber.ToInt16(a);
//	}

//	public static int ToInt32(DecimalNumber a)
//	{
//		return DecimalNumber.ToInt32(a);
//	}

//	public static long ToInt64(DecimalNumber a)
//	{
//		return DecimalNumber.ToInt64(a);
//	}

//	public static float ToFloatRelativeToOrigin(DecimalNumber a, DecimalNumber origin)
//	{
//		return DecimalNumber.ToFloatRelativeToOrigin(a, origin);
//	}

//	public static double ToDoubleRelativeToOrigin(DecimalNumber a, DecimalNumber origin)
//	{
//		return DecimalNumber.ToDoubleRelativeToOrigin(a, origin);
//	}

//	public DecimalNumber Add(DecimalNumber b)
//	{
//		return DecimalNumber.Add(b);
//	}

//	public DecimalNumber Subtract(DecimalNumber b)
//	{
//		return DecimalNumber.Subtract(b);
//	}

//	public DecimalNumber Multiply(DecimalNumber b)
//	{
//		return DecimalNumber.Multiply(b);
//	}

//	public DecimalNumber Divide(DecimalNumber b)
//	{
//		return DecimalNumber.Divide(b);
//	}

//	public DecimalNumber Negate()
//	{
//		return DecimalNumber.Negate();
//	}

//	public static DecimalNumber Add(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.Add(a, b);
//	}

//	public static DecimalNumber Subtract(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.Subtract(a, b);
//	}

//	public static DecimalNumber Multiply(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.Multiply(a, b);
//	}

//	public static DecimalNumber Divide(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.Divide(a, b);
//	}

//	public static DecimalNumber Negate(DecimalNumber a)
//	{
//		return DecimalNumber.Negate(a);
//	}

//	public static DecimalNumber Sqrt(DecimalNumber a)
//	{
//		return DecimalNumber.Sqrt(a);
//	}

//	public static DecimalNumber Abs(DecimalNumber a)
//	{
//		return DecimalNumber.Abs(a);
//	}

//	public static DecimalNumber Min(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.Min(a, b);
//	}

//	public static DecimalNumber Max(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.Max(a, b);
//	}

//	public static DecimalNumber Clamp(DecimalNumber value, DecimalNumber min, DecimalNumber max)
//	{
//		return DecimalNumber.Clamp(value, min, max);
//	}

//	public static DecimalNumber Floor(DecimalNumber a)
//	{
//		return DecimalNumber.Floor(a);
//	}

//	public static DecimalNumber Ceil(DecimalNumber a)
//	{
//		return DecimalNumber.Ceil(a);
//	}

//	public static DecimalNumber Round(DecimalNumber a)
//	{
//		return DecimalNumber.Round(a);
//	}

//	public static int Sign(DecimalNumber a)
//	{
//		return DecimalNumber.Sign(a);
//	}

//	public static DecimalNumber Sin(DecimalNumber a)
//	{
//		return DecimalNumber.Sin(a);
//	}

//	public static DecimalNumber Asin(DecimalNumber a)
//	{
//		return DecimalNumber.Asin(a);
//	}

//	public static DecimalNumber Cos(DecimalNumber a)
//	{
//		return DecimalNumber.Cos(a);
//	}

//	public static DecimalNumber Acos(DecimalNumber a)
//	{
//		return DecimalNumber.Acos(a);
//	}

//	public static DecimalNumber Tan(DecimalNumber a)
//	{
//		return DecimalNumber.Tan(a);
//	}

//	public static DecimalNumber Atan(DecimalNumber a)
//	{
//		return DecimalNumber.Atan(a);
//	}

//	public static DecimalNumber Random(Random random)
//	{
//		return DecimalNumber.Random(random);
//	}

//	public static DecimalNumber Random(Random random, DecimalNumber min, DecimalNumber max)
//	{
//		return DecimalNumber.Random(random, min, max);
//	}

//	public bool GreaterThan(DecimalNumber b)
//	{
//		return DecimalNumber.GreaterThan(b);
//	}

//	public bool GreaterThanOrEqual(DecimalNumber b)
//	{
//		return DecimalNumber.GreaterThanOrEqual(b);
//	}

//	public bool SmallerThan(DecimalNumber b)
//	{
//		return DecimalNumber.SmallerThan(b);
//	}

//	public bool SmallerThanOrEqual(DecimalNumber b)
//	{
//		return DecimalNumber.SmallerThanOrEqual(b);
//	}

//	public static bool GreaterThan(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.GreaterThan(a, b);
//	}

//	public static bool GreaterThanOrEqual(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.GreaterThanOrEqual(a, b);
//	}

//	public static bool SmallerThan(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.SmallerThan(a, b);
//	}

//	public static bool SmallerThanOrEqual(DecimalNumber a, DecimalNumber b)
//	{
//		return DecimalNumber.SmallerThanOrEqual(a, b);
//	}

//	public string ToString(string? format, IFormatProvider? formatProvider)
//	{
//		return DecimalNumber.ToString(format, formatProvider);
//	}

//	public bool Equals(DecimalNumber other)
//	{
//		return DecimalNumber.Equals(other);
//	}
//}
