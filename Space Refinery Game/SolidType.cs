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

	public SolidType(ChemicalType chemicalType, string solidName, FixedDecimalLong8 density) : base(chemicalType, solidName, density)
	{
	}
}