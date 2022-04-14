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

	public PlasmaType(ChemicalType chemicalType, string plasmaName, FixedDecimalInt4 density) : base(chemicalType, plasmaName, density)
	{
	}
}