using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

public static class UnitsMath
{
	public static TUnit Abs<TUnit>(this TUnit a)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Abs((DN)a);
	}

	public static TUnit Difference<TUnit>(this TUnit a, TUnit b)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Abs((DN)a - (DN)b);
	}

	public static TUnit Ceil<TUnit>(this TUnit a)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Ceil((DN)a);
	}

	public static TUnit Clamp<TUnit>(this TUnit value, TUnit min, TUnit max)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Clamp((DN)value, (DN)min, (DN)max);
	}

	public static TUnit Floor<TUnit>(this TUnit a)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Floor((DN)a);
	}

	public static TUnit Max<TUnit>(this TUnit a, TUnit b)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Max((DN)a, (DN)b);
	}

	public static TUnit Min<TUnit>(this TUnit a, TUnit b)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Min((DN)a, (DN)b);
	}

	public static TUnit Round<TUnit>(this TUnit a)
		where TUnit :
			IUnit<TUnit>
	{
		return (TUnit)DecimalNumber.Round((DN)a);
	}

	public static int Sign<TUnit>(this TUnit a)
		where TUnit :
			IUnit<TUnit>
	{
		return DecimalNumber.Sign((DN)a);
	}
}
