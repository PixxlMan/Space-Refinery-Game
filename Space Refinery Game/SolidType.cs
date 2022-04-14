using FixedPrecision;

namespace Space_Refinery_Game;

[Serializable]
public class SolidType : ResourceType
{
	public SolidType(ChemicalType chemicalType, string solidName, FixedDecimalInt4 density) : base(chemicalType, solidName, density)
	{
	}
}