using FixedPrecision;
using ImGuiNET;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

public sealed class GasType : ResourceType
{
	public GasType()
	{

	}

	public GasType(ChemicalType chemicalType, string gasName, DecimalNumber density, DecimalNumber specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Gas;
}