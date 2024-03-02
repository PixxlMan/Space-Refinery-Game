using Space_Refinery_Engine;
using Space_Refinery_Utilities;
using System.Diagnostics;

namespace Space_Refinery_Game;

public sealed class PhaseChangeReactionType : ReactionType
{
	public override string Reaction => "(s) -> (l), (l) -> (g), (g) -> (l), (l) -> (s)";

	// resourceUnitsToAdd is declared as a field in order to avoid having to recreate it each tick. It is always cleared after use.
	private List<ResourceUnitData> resourceUnitsToAdd = new();

	public override void Tick(IntervalUnit _, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> _2, ICollection<ReactionFactor> producedReactionFactors)
	{
		return; // Temporarily disabled while doing experiments

		lock (resourceContainer.SyncRoot)
		{
			foreach (var unit in resourceContainer.EnumerateResources())
			{
				ResourceType resourceType = unit.ResourceType;
				ChemicalType chemicalType = resourceType.ChemicalType;

				ResourceUnitData newUnit, changeFactorUnit;

				switch (unit.ResourceType.ChemicalPhase)
				{
					case ChemicalPhase.Solid:
						// Solid can go to:
						// - Liquid (if temp > tempOfFusion)
						if (unit.NonGasTemperature > resourceType.ChemicalType.TemperatureOfFusion)
						{
							TransitionPhase(unit, ChemicalPhase.Liquid, chemicalType.EnthalpyOfFusion,
								unit.InternalEnergy - ChemicalType.TemperatureToInternalEnergy(resourceType, chemicalType.TemperatureOfFusion, unit.Mass),
								out newUnit, out changeFactorUnit);
							producedReactionFactors.Add(new Melting());
							Logging.Log("Melting");
						}
						else
						{
							continue;
						}
						break;
					case ChemicalPhase.Liquid:
						// Liquid can go to:
						// - Gas (if temp > temperatureOfVaporization)
						// - Solid (if temp < temperatureOfFusion)
						if (unit.NonGasTemperature > resourceType.ChemicalType.TemperatureOfVaporization)
						{
							TransitionPhase(unit, ChemicalPhase.Gas, chemicalType.EnthalpyOfVaporization,
								unit.InternalEnergy - ChemicalType.TemperatureToInternalEnergy(resourceType, chemicalType.TemperatureOfVaporization, unit.Mass),
								out newUnit, out changeFactorUnit);
							producedReactionFactors.Add(new Vaporizing());
							Logging.Log("Vaporizing");
						}
						else if (unit.NonGasTemperature < resourceType.ChemicalType.TemperatureOfFusion)
						{
							TransitionPhase(unit, ChemicalPhase.Solid, chemicalType.EnthalpyOfFusion,
								ChemicalType.TemperatureToInternalEnergy(resourceType, chemicalType.TemperatureOfVaporization, unit.Mass) - unit.InternalEnergy,
								out newUnit, out changeFactorUnit);
							producedReactionFactors.Add(new Solidifying());
							Logging.Log("Solidifying");
						}
						else
						{
							continue;
						}
						break;
					case ChemicalPhase.Gas:
						// Gas can go to:
						// - Liquid (if temp < temperatureOfVaporization)
						// - Plasma (unsupported)
						if (unit.NonGasTemperature < resourceType.ChemicalType.TemperatureOfVaporization)
						{
							TransitionPhase(unit, ChemicalPhase.Liquid, chemicalType.EnthalpyOfVaporization,
								unit.InternalEnergy - ChemicalType.TemperatureToInternalEnergy(resourceType, chemicalType.TemperatureOfVaporization, unit.Mass),
								out newUnit, out changeFactorUnit);
							producedReactionFactors.Add(new Condensating());
							Logging.Log("Condensating");
						}
						else
						{
							continue;
						}
						break;
					default:
						throw new GlitchInTheMatrixException();
				}

				resourceUnitsToAdd.Add(newUnit);
				resourceUnitsToAdd.Add(changeFactorUnit);
			}

			// Resources should be added outside of the loop to avoid problems where the loop never terminates or does the same process multiple times or uses updated values, doing too many operations at once etc.
			resourceContainer.AddResources(resourceUnitsToAdd);
		}

		resourceUnitsToAdd.Clear();
	}

	private static void TransitionPhase(ResourceUnitData previousUnit, ChemicalPhase newPhase, MolarEnergyUnit phaseTransitionEnthalpy, EnergyUnit energyDelta, out ResourceUnitData newUnit, out ResourceUnitData changeFactorUnit)
	{
		throw new NotImplementedException();

		ResourceType previousResourceType = previousUnit.ResourceType;
		ChemicalType chemicalType = previousResourceType.ChemicalType;
		ResourceType newResourceType = chemicalType.GetResourceTypeForPhase(newPhase);

		Debug.Assert(phaseTransitionEnthalpy > 0);
		Debug.Assert(energyDelta > 0);
	}
}
