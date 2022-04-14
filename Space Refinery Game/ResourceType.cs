using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public abstract class ResourceType
{
	[NonSerialized]
	[JsonIgnore]
	public ChemicalType ChemicalType;

	public string ResourceName;

	public FixedDecimalInt4 Density;

	protected ResourceType(ChemicalType chemicalType, string resourceName, FixedDecimalInt4 density)
	{
		ChemicalType = chemicalType;
		ResourceName = resourceName;
		Density = density;
	}
}