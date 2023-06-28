using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game
{
	public sealed class LiquidType : ResourceType
	{
		public LiquidType()
		{

		}

		public LiquidType(ChemicalType chemicalType, string gasName, DecimalNumber density) : base(chemicalType, gasName, density)
		{
		}

		public override ChemicalPhase ChemicalPhase => ChemicalPhase.Liquid;
	}
}