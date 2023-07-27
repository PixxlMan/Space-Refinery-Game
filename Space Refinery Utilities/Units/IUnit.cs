using Space_Refinery_Game;
using System.Numerics;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// The purpose of Units is to achieve static type safety in physical and chemical calculations, with no runtime performance pentalty because units are removed and replaced by simple <c>DecimalNumbers</c> at runtime.
/// </summary>
public interface IUnit<TSelf> :
	IEquatable<TSelf>, // It should always be possible to perform equality comparasions between Units of the same type.
	IComparisonOperators<TSelf, TSelf, bool>, // It should always be possible to perform comparasions, such as greater than or smaller than or equal to between Units of the same type.
	IPortionable<TSelf> // It should always be possible to create a portion between two Units of the same type.
	where TSelf : IUnit<TSelf>
{
	public static abstract explicit operator DecimalNumber(TSelf unit);

	public static abstract explicit operator TSelf(DecimalNumber value);

	public static abstract implicit operator TSelf(int value);

	public static abstract implicit operator TSelf(double value);

	public static abstract bool operator ==(TSelf a, TSelf b);

	public static abstract bool operator !=(TSelf a, TSelf b);
}