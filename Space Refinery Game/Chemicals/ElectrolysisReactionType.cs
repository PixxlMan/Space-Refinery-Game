using Space_Refinery_Engine;

namespace Space_Refinery_Game;

public sealed class ElectrolysisReactionType : ReactionType // https://sv.wikipedia.org/wiki/Elektrolys, https://en.wikipedia.org/wiki/Electrolysis_of_water, https://en.wikipedia.org/wiki/Water_splitting
{
	public override string Reaction => "2 H₂O -> 2 H₂ + O₂"; // Wikipedia electrolysis of water @ Equations @ Overall reaction

	static CoulombUnit coulombForReaction => (molesOfWater * 2) * Electricity.FaradayConstant; // [J/Reaction] (wikipedia states that 2 electrons are required per mole of water)

	static DecimalNumber reactionScale => 1000; // Operate on a reaction model that is 1000x bigger.

	static MolesUnit molesOfWater => (MolesUnit)(2 * reactionScale); // [mol]

	public override void Tick(IntervalUnit interval, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> reactionFactors, ICollection<ReactionFactor> producedReactionFactors)
	{
		EnergyUnit electricalEnergy = (EnergyUnit)DecimalNumber.Zero;

		foreach (ElectricalCurrent electricalCurrent in reactionFactors[typeof(ElectricalCurrent)])
		{
			electricalEnergy += electricalCurrent.ElectricalEnergy;
		}

		Portion<CoulombUnit> electrolysisProcess = ((Electricity.ElectricalEnergyToCoulomb(electricalEnergy) * interval) / coulombForReaction);

		var water = resourceContainer.TakeResourceByMoles(ChemicalType.Water.LiquidPhaseType, UnitsMath.Min(resourceContainer.GetResourceUnitData(ChemicalType.Water.LiquidPhaseType).Moles, molesOfWater * (Portion<MolesUnit>)(DN)electrolysisProcess));

		water.BreakInto(2, out IReadOnlyDictionary<ResourceType, ResourceUnitData> resourceUnitDatas, (ChemicalType.Hydrogen.GasPhaseType, 2), (ChemicalType.Oxygen.GasPhaseType, 1));

		VolumeUnit totalVolume = (VolumeUnit)DecimalNumber.Zero;

		foreach (ResourceUnitData resourceUnit in resourceUnitDatas.Values)
		{
			totalVolume += resourceUnit.Volume;
		}

		// this isn't really thread safe? more stuff could be added between the volume check and adding...
		if (resourceContainer.NonCompressableVolume + totalVolume < resourceContainer.VolumeCapacity) // at least until pressure can be simulated...
		{
			resourceContainer.AddResources(resourceUnitDatas.Values);
		}
		else
		{
			resourceContainer.AddResource(water);
		}
	}
}
