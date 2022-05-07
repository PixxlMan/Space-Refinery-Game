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

		public LiquidType(ChemicalType chemicalType, string gasName, FixedDecimalLong8 density, FixedDecimalLong8 specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
		{
		}

		public override ChemicalPhase ChemicalPhase => ChemicalPhase.Liquid;
	}
}