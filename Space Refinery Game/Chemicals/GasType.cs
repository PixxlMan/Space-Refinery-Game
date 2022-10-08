using FixedPrecision;
using ImGuiNET;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public sealed class GasType : ResourceType
{
	[JsonConstructor]
	public GasType()
	{

	}

	public GasType(ChemicalType chemicalType, string gasName, DecimalNumber density, DecimalNumber specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Gas;
}