using FixedPrecision;

namespace Space_Refinery_Game;

[Serializable]
public class PlasmaType : ResourceType
{
	public PlasmaType(ChemicalType chemicalType, string plasmaName, FixedDecimalInt4 density) : base(chemicalType, plasmaName, density)
	{
	}
}