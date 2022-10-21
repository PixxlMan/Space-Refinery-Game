﻿using FixedPrecision;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

public sealed class SolidType : ResourceType
{
	public SolidType()
	{

	}

	public SolidType(ChemicalType chemicalType, string gasName, DecimalNumber density, DecimalNumber specificHeatCapacity) : base(chemicalType, gasName, density, specificHeatCapacity)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Solid;
}