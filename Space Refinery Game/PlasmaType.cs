using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public class PlasmaType : ResourceType
{
	[JsonConstructor]
	public PlasmaType()
	{
	}

	public PlasmaType(ChemicalType chemicalType, string plasmaName, FixedDecimalLong8 density) : base(chemicalType, plasmaName, density)
	{
	}
}