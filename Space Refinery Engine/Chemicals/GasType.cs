namespace Space_Refinery_Engine;

public sealed class GasType : ResourceType
{
	public GasType()
	{

	}

	public GasType(ChemicalType chemicalType, string gasName, DensityUnit density) : base(chemicalType, gasName, density)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Gas;
}