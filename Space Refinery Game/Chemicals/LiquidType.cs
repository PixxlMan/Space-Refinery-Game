using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game
{
	[Serializable]
	public sealed class LiquidType : ResourceType
	{
		[JsonConstructor]
		public LiquidType()
		{

		}

		public LiquidType(ChemicalType chemicalType, string gasName, DecimalNumber density, DecimalNumber specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
		{
		}

		public override ChemicalPhase ChemicalPhase => ChemicalPhase.Liquid;
	}
}