using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public sealed class SolidType : ResourceType
{
	[JsonConstructor]
	public SolidType()
	{

	}

	public SolidType(ChemicalType chemicalType, string gasName, FixedDecimalLong8 density, FixedDecimalLong8 specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Solid;
}