namespace Space_Refinery_Game;

public sealed class ElectrolysisReactionType : ReactionType // https://sv.wikipedia.org/wiki/Elektrolys, https://en.wikipedia.org/wiki/Electrolysis_of_water, https://en.wikipedia.org/wiki/Water_splitting
{
	public override string Reaction => "2 H₂O -> 2 H₂ + O₂"; // Wikipedia electrolysis of water @ Equations @ Overall reaction

	static DecimalNumber coulombForReaction => molesOfWater * 2 * Electricity.FaradayConstant; // [J/Reaction] (wikipedia states that 2 electrons are required per mole of water)

	static DecimalNumber reactionScale => 1000; // Operate on a reaction model that is 1000x bigger.

	static DecimalNumber molesOfWater => 2 * reactionScale; // [mol]

	public override void Tick(DecimalNumber interval, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> reactionFactors, ICollection<ReactionFactor> producedReactionFactors)
	{
		EnergyUnit electricalEnergy = (EnergyUnit)DecimalNumber.Zero;

		foreach (ElectricalCurrent electricalCurrent in reactionFactors[typeof(ElectricalCurrent)])
		{
			electricalEnergy += electricalCurrent.ElectricalEnergy;
		}

		var electrolysisProcess = (Electricity.ElectricalEnergyToCoulomb(electricalEnergy) / coulombForReaction) * interval;

		var water = resourceContainer.TakeResourceByMoles(ChemicalType.Water.LiquidPhaseType, DecimalNumber.Min((DecimalNumber)resourceContainer.GetResourceUnitData(ChemicalType.Water.LiquidPhaseType).Moles, molesOfWater * electrolysisProcess));

		water.BreakInto(2, out IReadOnlyDictionary<ResourceType, ResourceUnitData> resourceUnitDatas, (ChemicalType.Hydrogen.GasPhaseType, 2), (ChemicalType.Oxygen.GasPhaseType, 1));

		VolumeUnit totalVolume = (VolumeUnit)DecimalNumber.Zero;

		foreach (ResourceUnitData resourceUnit in resourceUnitDatas.Values)
		{
			totalVolume += resourceUnit.Volume;
		}

		// this isn't really thread safe? more stuff could be added between the volume check and adding...
		if (resourceContainer.Volume + totalVolume < resourceContainer.MaxVolume) // at least until pressure can be simulated...
		{
			resourceContainer.AddResources(resourceUnitDatas.Values);
		}
		else
		{
			resourceContainer.AddResource(water);
		}
	}
}
