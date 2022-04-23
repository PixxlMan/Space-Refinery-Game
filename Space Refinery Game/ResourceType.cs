﻿using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public abstract class ResourceType
{
	[NonSerialized]
	[JsonIgnore]
	public ChemicalType ChemicalType;

	public string ResourceName;

	public FixedDecimalLong8 Density; // kg/m3

	public FixedDecimalInt4 SpecificHeatCapacity; // j/k

	protected ResourceType()
	{

	}

	protected ResourceType(ChemicalType chemicalType, string resourceName, FixedDecimalLong8 density)
	{
		ChemicalType = chemicalType;
		ResourceName = resourceName;
		Density = density;
	}
}