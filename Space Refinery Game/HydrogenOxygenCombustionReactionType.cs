using System.Collections.Concurrent;

namespace Space_Refinery_Game
{
	public sealed class HydrogenOxygenCombustionReactionType : ReactionType // https://en.wikipedia.org/wiki/Oxyhydrogen
	{
		public override string Reaction => "2 H₂ + O₂ -> 2 H₂O";

		private static ConcurrentDictionary<ResourceContainer, object> currentlySustainingReactions = new();

		public override void Tick(DecimalNumber interval, ResourceContainer resourceContainer, IEnumerable<ReactionFactor> reactionFactors)
		{
			if (!currentlySustainingReactions.ContainsKey(resourceContainer))
			{
				var sparks = reactionFactors.OfType<Spark>();

				if (sparks.Count() == 0)
				{
					return;
				}

				bool shouldStartToCombust = false;

				/*check temperature when implemented... autoignition temperature is 570 °C*/

				foreach (var spark in sparks)
				{
					if (spark.SparkEnergy > 20 * DecimalNumber.Micro)
					{
						shouldStartToCombust = true;

						break;
					}
				}

				if (shouldStartToCombust)
				{
					currentlySustainingReactions.AddUnique(resourceContainer, null);
				}
				else
				{
					return;
				}
			}
			else
			{
				if (resourceContainer.MolesOf(ChemicalType.Hydrogen.GasPhaseType) == DecimalNumber.Zero || resourceContainer.MolesOf(ChemicalType.Oxygen.GasPhaseType) == DecimalNumber.Zero)
				{
					currentlySustainingReactions.RemoveStrict(resourceContainer);

					return;
				}

				var oxygen = resourceContainer.TakeAllResource(ChemicalType.Oxygen.GasPhaseType);

				var hydrogen = resourceContainer.TakeAllResource(ChemicalType.Hydrogen.GasPhaseType);

				// there is no need to check whether the output product will fit volume wise since we know that the volume of produced water is always smaller.

				if (hydrogen.Moles * 2 > oxygen.Moles)
				{ // oxygen limited
					resourceContainer.AddResource(new ResourceUnitData(ChemicalType.Water.LiquidPhaseType, oxygen.Moles));
				}
				else
				{ // hydrogen limited
					resourceContainer.AddResource(new ResourceUnitData(ChemicalType.Water.LiquidPhaseType, hydrogen.Moles * 2));
				}
			}
		}
	}
}
