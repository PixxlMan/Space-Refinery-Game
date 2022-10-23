using FixedPrecision;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game
{
	[JsonConverter(typeof(DecimalNumberJsonConverter))]
	public struct DecimalNumber : IFixedPrecisionNumeral<DecimalNumber>
	{
		public DecimalNumber(FixedDecimalLong8 value)
		{
			Value = value;
		}

		public readonly FixedDecimalLong8 Value;

		public static byte Precision => FixedDecimalLong8.Precision;

		public static DecimalNumber MaxValue => FixedDecimalLong8.MaxValue;

		public static DecimalNumber SmallestIncrement => FixedDecimalLong8.SmallestIncrement;

		public static DecimalNumber One => FixedDecimalLong8.One;

		public static DecimalNumber Zero => FixedDecimalLong8.Zero;

		public static DecimalNumber PI => FixedDecimalLong8.PI;

		public static DecimalNumber DegreesToRadians => FixedDecimalLong8.DegreesToRadians;

		public static DecimalNumber RadiansToDegrees => FixedDecimalLong8.RadiansToDegrees;

		public static DecimalNumber Abs(DecimalNumber a)
		{
			return FixedDecimalLong8.Abs(a);
		}

		public static DecimalNumber Acos(DecimalNumber a)
		{
			return FixedDecimalLong8.Acos(a);
		}

		public static DecimalNumber Add(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.Add(a, b);
		}

		public static DecimalNumber Asin(DecimalNumber a)
		{
			return FixedDecimalLong8.Asin(a);
		}

		public static DecimalNumber Difference(DecimalNumber time1, DecimalNumber time2)
		{
			return Abs(time1 - time2);
		}

		public static DecimalNumber Atan(DecimalNumber a)
		{
			return FixedDecimalLong8.Atan(a);
		}

		public static DecimalNumber Ceil(DecimalNumber a)
		{
			return FixedDecimalLong8.Ceil(a);
		}

		public static DecimalNumber Clamp(DecimalNumber value, DecimalNumber min, DecimalNumber max)
		{
			return FixedDecimalLong8.Clamp(value, min, max);
		}

		public static DecimalNumber Cos(DecimalNumber a)
		{
			return FixedDecimalLong8.Cos(a);
		}

		public static DecimalNumber Divide(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.Divide(a, b);
		}

		public static DecimalNumber Floor(DecimalNumber a)
		{
			return FixedDecimalLong8.Floor(a);
		}

		public static DecimalNumber FromDecimal(decimal value)
		{
			return FixedDecimalLong8.FromDecimal(value);
		}

		public static DecimalNumber FromDouble(double value)
		{
			return FixedDecimalLong8.FromDouble(value);
		}

		public static DecimalNumber FromFloat(float value)
		{
			return FixedDecimalLong8.FromFloat(value);
		}

		public static DecimalNumber FromInt16(short value)
		{
			return FixedDecimalLong8.FromInt16(value);
		}

		public static DecimalNumber FromInt32(int value)
		{
			return FixedDecimalLong8.FromInt32(value);
		}

		public static DecimalNumber FromInt64(long value)
		{
			return FixedDecimalLong8.FromInt64(value);
		}

		public TimeSpan ToTimeSpan()
		{
			return TimeSpan.FromSeconds(ToDouble());
		}

		public static bool GreaterThan(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.GreaterThan(a, b);
		}

		public static bool GreaterThanOrEqual(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.GreaterThanOrEqual(a, b);
		}

		public static DecimalNumber Max(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.Max(a, b);
		}

		public static DecimalNumber Min(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.Min(a, b);
		}

		public static DecimalNumber Multiply(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.Multiply(a, b);
		}

		public static DecimalNumber Negate(DecimalNumber a)
		{
			return FixedDecimalLong8.Negate(a);
		}

		public static DecimalNumber Parse(string value)
		{
			return FixedDecimalLong8.Parse(value);
		}

		public static DecimalNumber Random(Random random)
		{
			return FixedDecimalLong8.Random(random);
		}

		public static DecimalNumber Random(Random random, DecimalNumber min, DecimalNumber max)
		{
			return FixedDecimalLong8.Random(random, min, max);
		}

		public static DecimalNumber Round(DecimalNumber a)
		{
			return FixedDecimalLong8.Round(a);
		}

		public static DecimalNumber FromTimeSpan(TimeSpan duration)
		{
			return FromDouble(duration.TotalSeconds);
		}

		public static int Sign(DecimalNumber a)
		{
			return FixedDecimalLong8.Sign(a);
		}

		public static DecimalNumber Sin(DecimalNumber a)
		{
			return FixedDecimalLong8.Sin(a);
		}

		public static bool SmallerThan(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.SmallerThan(a, b);
		}

		public static bool SmallerThanOrEqual(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.SmallerThanOrEqual(a, b);
		}

		public static DecimalNumber Sqrt(DecimalNumber a)
		{
			return FixedDecimalLong8.Sqrt(a);
		}

		public static DecimalNumber Subtract(DecimalNumber a, DecimalNumber b)
		{
			return FixedDecimalLong8.Subtract(a, b);
		}

		public static DecimalNumber Tan(DecimalNumber a)
		{
			return FixedDecimalLong8.Tan(a);
		}

		public static decimal ToDecimal(DecimalNumber a)
		{
			return FixedDecimalLong8.ToDecimal(a);
		}

		public static double ToDouble(DecimalNumber a)
		{
			return FixedDecimalLong8.ToDouble(a);
		}

		public static double ToDoubleRelativeToOrigin(DecimalNumber a, DecimalNumber origin)
		{
			return FixedDecimalLong8.ToDoubleRelativeToOrigin(a, origin);
		}

		public static float ToFloat(DecimalNumber a)
		{
			return FixedDecimalLong8.ToFloat(a);
		}

		public static float ToFloatRelativeToOrigin(DecimalNumber a, DecimalNumber origin)
		{
			return FixedDecimalLong8.ToFloatRelativeToOrigin(a, origin);
		}

		public static short ToInt16(DecimalNumber a)
		{
			return FixedDecimalLong8.ToInt16(a);
		}

		public static int ToInt32(DecimalNumber a)
		{
			return FixedDecimalLong8.ToInt32(a);
		}

		public static long ToInt64(DecimalNumber a)
		{
			return FixedDecimalLong8.ToInt64(a);
		}

		public static bool TryParse(string value, out DecimalNumber result)
		{
			var success = FixedDecimalLong8.TryParse(value, out var resultFixed);

			result = resultFixed;

			return success;
		}

		public DecimalNumber Add(DecimalNumber b)
		{
			return Value.Add(b);
		}

		public DecimalNumber Divide(DecimalNumber b)
		{
			return Value.Divide(b);
		}

		public bool Equals(DecimalNumber? other)
		{
			return Value.Equals(other);
		}

		public bool GreaterThan(DecimalNumber b)
		{
			return Value.GreaterThan(b);
		}

		public bool GreaterThanOrEqual(DecimalNumber b)
		{
			return Value.GreaterThanOrEqual(b);
		}

		public DecimalNumber Multiply(DecimalNumber b)
		{
			return Value.Multiply(b);
		}

		public DecimalNumber Negate()
		{
			return Value.Negate();
		}

		public bool SmallerThan(DecimalNumber b)
		{
			return Value.SmallerThan(b);
		}

		public bool SmallerThanOrEqual(DecimalNumber b)
		{
			return Value.SmallerThanOrEqual(b);
		}

		public DecimalNumber Subtract(DecimalNumber b)
		{
			return Value.Subtract(b);
		}

		public decimal ToDecimal()
		{
			return Value.ToDecimal();
		}

		public double ToDouble()
		{
			return Value.ToDouble();
		}

		public double ToDoubleRelativeToOrigin(DecimalNumber origin)
		{
			return Value.ToDoubleRelativeToOrigin(origin);
		}

		public float ToFloat()
		{
			return Value.ToFloat();
		}

		public float ToFloatRelativeToOrigin(DecimalNumber origin)
		{
			return Value.ToFloatRelativeToOrigin(origin);
		}

		public short ToInt16()
		{
			return Value.ToInt16();
		}

		public int ToInt32()
		{
			return Value.ToInt32();
		}

		public long ToInt64()
		{
			return Value.ToInt64();
		}

		public override string ToString()
		{
			return Value.ToString(null, null);
		}

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			return Value.ToString(format, formatProvider);
		}

		public bool Equals(DecimalNumber other)
		{
			return Value.Equals(other.Value);
		}

		public static DecimalNumber operator +(DecimalNumber left, DecimalNumber right)
		{
			return left.Value + right.Value;
		}

		public static DecimalNumber operator -(DecimalNumber left)
		{
			return -left.Value;
		}

		public static DecimalNumber operator -(DecimalNumber left, DecimalNumber right)
		{
			return left.Value - right.Value;
		}

		public static DecimalNumber operator ++(DecimalNumber other)
		{
			return other.Value + 1;
		}

		public static DecimalNumber operator --(DecimalNumber other)
		{
			return other.Value + 1;
		}

		public static DecimalNumber operator *(DecimalNumber left, DecimalNumber right)
		{
			return left.Value * right.Value;
		}

		public static DecimalNumber operator /(DecimalNumber left, DecimalNumber right)
		{
			return left.Value / right.Value;
		}

		public static bool operator ==(DecimalNumber left, DecimalNumber right)
		{
			return left.Value == right.Value;
		}

		public static bool operator !=(DecimalNumber left, DecimalNumber right)
		{
			return left.Value != right.Value;
		}

		public static bool operator <(DecimalNumber left, DecimalNumber right)
		{
			return left.Value < right.Value;
		}

		public static bool operator >(DecimalNumber left, DecimalNumber right)
		{
			return left.Value > right.Value;
		}

		public static bool operator <=(DecimalNumber left, DecimalNumber right)
		{
			return left.Value <= right.Value;
		}

		public static bool operator >=(DecimalNumber left, DecimalNumber right)
		{
			return left.Value >= right.Value;
		}

		public static implicit operator FixedDecimalLong8(DecimalNumber time)
		{
			return time.Value;
		}

		public static implicit operator DecimalNumber(FixedDecimalLong8 value)
		{
			return new(value);
		}

		public static explicit operator DecimalNumber(TimeSpan value)
		{
			return FromTimeSpan(value);
		}
		
		public static implicit operator FixedDecimalInt4(DecimalNumber value) => ToFixedDecimalInt4(value);

		public static implicit operator DecimalNumber(FixedDecimalInt4 value) => FromFixedDecimalInt4(value);

		const int fixedDecimalInt4PrecisionDifferenceFactor = 10 * 10 * 10 * 10; //(int)Math.Pow(Precision - FixedDecimalInt4.Precision, 10)

		public static FixedDecimalInt4 ToFixedDecimalInt4(DecimalNumber value)
		{
			return FixedDecimalInt4.FromDecimal(value.ToDecimal());
			//return FixedDecimalInt4.CreateInstanceFromRaw((int)(value.Value.GetRawValue() / FixedDecimalInt4PrecisionDifferenceFactor));
		}

		public static DecimalNumber FromFixedDecimalInt4(FixedDecimalInt4 value)
		{
			return FromDecimal(value.ToDecimal());
			//return new(value.GetRawValue() * FixedDecimalInt4PrecisionDifferenceFactor);
		}

		public static implicit operator DecimalNumber(short value) => FromInt16(value);
		public static implicit operator DecimalNumber(int value) => FromInt32(value);
		public static implicit operator DecimalNumber(long value) => FromInt64(value);

		public static explicit operator DecimalNumber(decimal value) => FromDecimal(value);
		public static implicit operator DecimalNumber(double value) => FromDouble(value);
		public static explicit operator DecimalNumber(float value) => FromFloat(value);

		public static explicit operator short(DecimalNumber value) => ToInt16(value);
		public static explicit operator int(DecimalNumber value) => ToInt32(value);
		public static explicit operator long(DecimalNumber value) => ToInt64(value);

		public static explicit operator decimal(DecimalNumber value) => ToDecimal(value);
		public static explicit operator double(DecimalNumber value) => ToDouble(value);
		public static explicit operator float(DecimalNumber value) => ToFloat(value);
	}

	public class DecimalNumberJsonConverter : JsonConverter<DecimalNumber>
	{
		public override DecimalNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return FixedDecimalLong8.FromDecimal(reader.GetDecimal());
		}

		public override void Write(Utf8JsonWriter writer, DecimalNumber value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue(value.ToDecimal());
		}
	}
}