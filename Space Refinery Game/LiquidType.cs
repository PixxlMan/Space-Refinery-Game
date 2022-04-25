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

		public LiquidType(ChemicalType chemicalType, string gasName, FixedDecimalLong8 density, FixedDecimalInt4 specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
		{
		}
	}
}