namespace Space_Refinery_Game
{
	public sealed class LiquidType : ResourceType
	{
		public LiquidType()
		{

		}

		public LiquidType(ChemicalType chemicalType, string gasName, DensityUnit density) : base(chemicalType, gasName, density)
		{
		}

		public override ChemicalPhase ChemicalPhase => ChemicalPhase.Liquid;
	}
}