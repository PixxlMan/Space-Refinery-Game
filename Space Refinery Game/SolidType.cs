using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public class SolidType : ResourceType
{
	[JsonConstructor]
	public SolidType()
	{

	}

	public SolidType(ChemicalType chemicalType, string gasName, FixedDecimalLong8 density, FixedDecimalInt4 specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
	{
	}
}