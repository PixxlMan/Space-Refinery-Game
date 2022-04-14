using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public class GasType : ResourceType
{
	[JsonConstructor]
	public GasType()
	{

	}

	public GasType(ChemicalType chemicalType, string gasName, FixedDecimalLong8 density) : base(chemicalType, gasName, density)
	{
	}
}