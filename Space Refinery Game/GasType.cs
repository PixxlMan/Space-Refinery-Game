using FixedPrecision;

namespace Space_Refinery_Game;

[Serializable]
public class GasType : ResourceType
{
	public GasType(ChemicalType chemicalType, string gasName, FixedDecimalInt4 density) : base(chemicalType, gasName, density)
	{
	}
}