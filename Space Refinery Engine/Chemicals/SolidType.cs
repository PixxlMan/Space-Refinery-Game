﻿namespace Space_Refinery_Engine;

public sealed class SolidType : ResourceType
{
	public SolidType()
	{

	}

	public SolidType(ChemicalType chemicalType, string gasName, DensityUnit density) : base(chemicalType, gasName, density)
	{
	}

	public override ChemicalPhase ChemicalPhase => ChemicalPhase.Solid;
}