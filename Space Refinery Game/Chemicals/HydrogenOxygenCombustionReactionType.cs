using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Space_Refinery_Game
{
	public sealed class HydrogenOxygenCombustionReactionType : ReactionType // https://en.wikipedia.org/wiki/Oxyhydrogen
	{
		public override string Reaction => "2 H₂ + O₂ -> 2 H₂O";

		public override void Tick(DecimalNumber interval, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> reactionFactors, ICollection<ReactionFactor> producedReactionFactors)
		{
			if (resourceContainer.MolesOf(ChemicalType.Hydrogen.GasPhaseType) == DecimalNumber.Zero || resourceContainer.MolesOf(ChemicalType.Oxygen.GasPhaseType) == DecimalNumber.Zero)
			{
				return;
			}

			if (!reactionFactors.Contains(typeof(Fire)))
			{
				/*check temperature when implemented... autoignition temperature is 570 °C*/
				if (reactionFactors.Contains(typeof(Spark)))
				{
					bool shouldStartToCombust = false;

					foreach (Spark spark in reactionFactors[typeof(Spark)])
					{
						if (spark.SparkEnergy >= 20 * DecimalNumber.Micro) // If the spark has more than 20 microjoules of energy, it will ignite the gas.
						{
							shouldStartToCombust = true;

							break;
						}
					}

					if (shouldStartToCombust)
					{
						producedReactionFactors.Add(new Fire());
					}
					else
					{
						return;
					}
				}
			}

			var oxygen = resourceContainer.TakeAllResource(ChemicalType.Oxygen.GasPhaseType);

			var hydrogen = resourceContainer.TakeAllResource(ChemicalType.Hydrogen.GasPhaseType);

			// there is no need to check whether the output product will fit volume wise since we know that the volume of produced water is always smaller.

			if (hydrogen.Moles * 2 > oxygen.Moles)
			{ // oxygen limited
				resourceContainer.AddResource(new ResourceUnitData(ChemicalType.Water.LiquidPhaseType, oxygen.Moles));
				resourceContainer.AddResource(new ResourceUnitData(ChemicalType.Hydrogen.GasPhaseType, hydrogen.Moles - oxygen.Moles * 2)); // Add back the hydrogen that didn't get used up.
			}
			else
			{ // hydrogen limited
				resourceContainer.AddResource(new ResourceUnitData(ChemicalType.Water.LiquidPhaseType, hydrogen.Moles * 2));
				resourceContainer.AddResource(new ResourceUnitData(ChemicalType.Oxygen.GasPhaseType, oxygen.Moles - hydrogen.Moles / 2)); // Add back the oxygen that didn't get used up.
			}
		}
	}
}
