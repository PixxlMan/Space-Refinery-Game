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

	public SolidType(ChemicalType chemicalType, string solidName, FixedDecimalInt4 density) : base(chemicalType, solidName, density)
	{
	}
}