using FixedPrecision;

namespace Space_Refinery_Game
{
	public interface IFluid
	{
		public static abstract FixedDecimalInt4 Density { get; }

		public FixedDecimalInt4 InstanceDensity { get; }
	}
}