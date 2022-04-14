using FixedPrecision;

namespace Space_Refinery_Game
{
	[Serializable]
	public class LiquidType : ResourceType
	{
		public LiquidType(ChemicalType chemicalType, string liquidName, FixedDecimalInt4 density) : base(chemicalType, liquidName, density)
		{
		}
	}
}