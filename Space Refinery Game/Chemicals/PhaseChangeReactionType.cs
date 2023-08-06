using Space_Refinery_Utilities;
using System.Diagnostics;
using Vortice.MediaFoundation;

namespace Space_Refinery_Game;

public sealed class PhaseChangeReactionType : ReactionType
{
	public override string Reaction => "(s) -> (l), (l) -> (g), (g) -> (l), (l) -> (s)";

	// resourceUnitsToAdd is declared as a field in order to avoid having to recreate it each tick. It is always cleared after use.
	private List<ResourceUnitData> resourceUnitsToAdd = new();

	public override void Tick(IntervalUnit _, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> _2, ICollection<ReactionFactor> producedReactionFactors)
	{
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
						if (unit.Temperature > resourceType.ChemicalType.TemperatureOfFusion)
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
						if (unit.Temperature > resourceType.ChemicalType.TemperatureOfVaporization)
						{
							TransitionPhase(unit, ChemicalPhase.Gas, chemicalType.EnthalpyOfVaporization,
								unit.InternalEnergy - ChemicalType.TemperatureToInternalEnergy(resourceType, chemicalType.TemperatureOfVaporization, unit.Mass),
								out newUnit, out changeFactorUnit);
							producedReactionFactors.Add(new Vaporizing());
							Logging.Log("Vaporizing");
						}
						else if (unit.Temperature < resourceType.ChemicalType.TemperatureOfFusion)
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
						if (unit.Temperature < resourceType.ChemicalType.TemperatureOfVaporization)
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="previousUnit"></param>
	/// <param name="newPhase"></param>
	/// <param name="phaseTransitionEnthalpy">[J/kg]</param>
	/// <param name="energyDelta">[J]</param>
	/// <param name="newUnit"></param>
	/// <param name="changeFactorUnit"></param>
	private static void TransitionPhase(ResourceUnitData previousUnit, ChemicalPhase newPhase, MolarEnergyUnit phaseTransitionEnthalpy, EnergyUnit energyDelta, out ResourceUnitData newUnit, out ResourceUnitData changeFactorUnit)
	{
		ResourceType previousResourceType = previousUnit.ResourceType;
		ChemicalType chemicalType = previousResourceType.ChemicalType;
		ResourceType newResourceType = chemicalType.GetResourceTypeForPhase(newPhase);

		Debug.Assert(phaseTransitionEnthalpy > 0);
		Debug.Assert(energyDelta > 0);

		// Upstep.
		//if (newPhase > previousResourceType.ChemicalPhase) // todo: start calculating temperature based on energy past the previous phase. The current implementation is incorrect.
		{
			// E = energy delta, internal energy added that exceeds the maximum energy to stay in phase
			// C = enthalpy of fusion or vaporization
			// m = mass
			// E = C * m
			//
			// In units:
			// [J] = [J/kg] * [kg]
			//
			// Solve for m, in order to get mass in new phase
			// m = E / C
			//
			// In units:
			// [kg] = [J] / [J/kg]

			var E = energyDelta; // [J]
			var C = phaseTransitionEnthalpy; // [J/kg]

			var m = E / C; // [kg] = [J] / [J/kg]

			// THIS IS WRONG - IT IS [mol], not [kg]! Check code!

			if (m > previousUnit.Mass)
			{
				m = previousUnit.Mass;

				E = C * m; // [J] = [J/kg] * [kg]
			}

			newUnit = new(
				resourceType: newResourceType,
				moles: ChemicalType.MassToMoles(chemicalType, m),
				internalEnergy: E);
			changeFactorUnit = ResourceUnitData.CreateNegativeResourceUnit(previousResourceType, -newUnit.Moles, -newUnit.InternalEnergy);

			return;
		}
		// Downstep.
		/*else
		{ // todo: complete if necessary
			// E = energy delta, internal energy removed that is below the minumum energy to stay in phase
			// C = enthalpy of fusion or vaporization
			// m = mass
			// E = C * m
			//
			// In units:
			// [J] = [J/kg] * [kg]
			//
			// Solve for m, in order to get mass in new phase
			// m = E / C
			//
			// In units:
			// [kg] = [J] / [J/kg]

			var E = energyDelta; // [J]
			var C = phaseTransitionEnthalpy; // [J/kg]

			var m = E / C; // [kg] = [J] / [J/kg]

			newUnit = new(newResourceType, ChemicalType.MassToMoles(chemicalType, m), E);
			changeFactorUnit = ResourceUnitData.CreateNegativeResourceUnit(previousResourceType, -newUnit.Moles, -newUnit.InternalEnergy);

			return;
		}*/
	}
}
