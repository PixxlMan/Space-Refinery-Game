using FixedPrecision;
using ImGuiNET;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public class GasType : ResourceType
{
	[JsonConstructor]
	public GasType()
	{

	}

	public GasType(ChemicalType chemicalType, string gasName, FixedDecimalLong8 density, FixedDecimalLong8 specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Gas;
}