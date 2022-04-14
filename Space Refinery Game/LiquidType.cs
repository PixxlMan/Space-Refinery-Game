using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game
{
	[Serializable]
	public class LiquidType : ResourceType
	{
		[JsonConstructor]
		public LiquidType()
		{

		}

		public LiquidType(ChemicalType chemicalType, string liquidName, FixedDecimalLong8 density) : base(chemicalType, liquidName, density)
		{
		}
	}
}