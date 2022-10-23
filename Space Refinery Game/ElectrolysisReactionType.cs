namespace Space_Refinery_Game
{
	public sealed class ElectrolysisReactionType : ReactionType // https://sv.wikipedia.org/wiki/Elektrolys, https://en.wikipedia.org/wiki/Electrolysis_of_water, https://en.wikipedia.org/wiki/Water_splitting
	{
		DecimalNumber electricalEnergy;

		public override string Reaction => "2 H₂O -> 2 H₂ + O₂"; // Wikipedia electrolysis of water @ Equations @ Overall reaction

		static DecimalNumber coulombForReaction => molesOfWater * 2 * Electricity.FaradayConstant; // [J/Reaction] (wikipedia states that 2 electrons are required per mole of water)

		static DecimalNumber reactionScale = 1000; // Operate on a reaction model that is 1000x bigger.

		static DecimalNumber molesOfWater = 2 * reactionScale; // [mol]

		public void ProvideElectricalPower(DecimalNumber electricalEnergy)
		{
			this.electricalEnergy += electricalEnergy;
		}

		public override void Tick(DecimalNumber interval, ResourceContainer resourceContainer)
		{
			var electrolysisProcess = (Electricity.ElectricalEnergyToCoulomb(electricalEnergy) / coulombForReaction) * interval;

			var water = resourceContainer.ExtractResourceByMoles(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType, molesOfWater * electrolysisProcess);

			resourceContainer.AddResource(new ResourceUnit(MainGame.ChemicalTypesDictionary["Hydrogen"].GasPhaseType, water.Moles, water.InternalEnergy / 2));
			resourceContainer.AddResource(new ResourceUnit(MainGame.ChemicalTypesDictionary["Oxygen"].GasPhaseType, water.Moles / 2, water.InternalEnergy / 2));
		}
	}
}
