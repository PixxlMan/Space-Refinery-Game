using Space_Refinery_Game;

namespace Space_Refinery_Utilities.Units;

/// <summary>
/// The purpose of Units is to achieve static type safety in physical and chemical calculations, with no runtime performance loss because units are removed and replaced by simple DecimalNumbers at runtime.
/// </summary>
internal interface IUnit<TSelf> where TSelf : IUnit<TSelf>
{
	public static abstract explicit operator DecimalNumber(TSelf unit);

	public static abstract explicit operator TSelf(DecimalNumber value);

	public static abstract implicit operator TSelf(int value);
}
