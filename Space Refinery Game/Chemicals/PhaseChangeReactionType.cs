using Space_Refinery_Utilities;
using System.Diagnostics;
using Vortice.MediaFoundation;

namespace Space_Refinery_Game;

public sealed class PhaseChangeReactionType : ReactionType
{
	public override string Reaction => "(s) -> (l), (l) -> (g), (g) -> (l), (l) -> (s)";

	public override void Tick(DecimalNumber _, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> _2, ICollection<ReactionFactor> producedReactionFactors)
	{
		lock (resourceContainer.SyncRoot)
		{
			foreach (var unit in resourceContainer.EnumerateResources())
			{
				ResourceType type = unit.ResourceType;
				ChemicalType chemicalType = type.ChemicalType;

				switch (unit.ResourceType.ChemicalPhase)
				{
					case ChemicalPhase.Solid:
						// Solid can go to:
						// - Liquid (if temp > tempOfFusion)
						if (unit.Temperature > type.ChemicalType.TemperatureOfFusion)
						{
							TransitionPhase(resourceContainer, unit, ChemicalPhase.Liquid, chemicalType.TemperatureOfFusion, chemicalType.EnthalpyOfFusion);
							producedReactionFactors.Add(new Melting());
							Logging.Log("Melting");
						}
						break;
					case ChemicalPhase.Liquid:
						// Liquid can go to:
						// - Gas (if temp > temperatureOfVaporization)
						// - Solid (if temp < temperatureOfFusion)
						if (unit.Temperature > type.ChemicalType.TemperatureOfVaporization)
						{
							TransitionPhase(resourceContainer, unit, ChemicalPhase.Gas, chemicalType.TemperatureOfVaporization, chemicalType.EnthalpyOfVaporization);
							producedReactionFactors.Add(new Vaporizing());
							Logging.Log("Vaporizing");
						}
						else if (unit.Temperature < type.ChemicalType.TemperatureOfFusion)
						{
							TransitionPhase(resourceContainer, unit, ChemicalPhase.Solid, chemicalType.TemperatureOfFusion, chemicalType.EnthalpyOfFusion);
							producedReactionFactors.Add(new Solidifying());
							Logging.Log("Solidifying");
						}
						break;
					case ChemicalPhase.Gas:
						// Gas can go to:
						// - Liquid (if temp < temperatureOfVaporization)
						// - Plasma (unsupported)
						if (unit.Temperature < type.ChemicalType.TemperatureOfVaporization)
						{
							TransitionPhase(resourceContainer, unit, ChemicalPhase.Liquid, chemicalType.TemperatureOfVaporization, chemicalType.EnthalpyOfVaporization);
							producedReactionFactors.Add(new Condensating());
							Logging.Log("Condensating");
						}
						break;
					default:
						throw new GlitchInTheMatrixException();
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="resourceContainer"></param>
	/// <param name="unit"></param>
	/// <param name="targetPhase"></param>
	/// <param name="temperatureOfPhaseTransition"></param>
	/// <param name="phaseTransitionEnthalpy">[J/mol]</param>
	private static void TransitionPhase(ResourceContainer resourceContainer, ResourceUnitData unit, ChemicalPhase targetPhase, DecimalNumber temperatureOfPhaseTransition, DecimalNumber phaseTransitionEnthalpy)
	{
		ResourceType type = unit.ResourceType;
		ChemicalType chemicalType = type.ChemicalType;
		ResourceType targetType = chemicalType.GetResourceTypeForPhase(targetPhase);

		if (targetPhase > type.ChemicalPhase) // todo: start calculating temperature based on energy past the previous phase. The current implementation is incorrect.
		{
			// The internal energy level that would be requited to start the transition. Calculated by taking the internal energy level of the phase transition start.
			DecimalNumber energyLevelAtStartOfPhaseTransition = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass);
			// The internal energy level that would be required for a complete phase transition. Calculated by taking the internal energy level of the start of the transition and then adding the energy required for the entire transition.
			DecimalNumber energyLevelAtCompletePhaseTransition = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass) + phaseTransitionEnthalpy * unit.Moles;

			// The amount of energy that is used for the phase transition. If there is more internal energy than is needed for a full phase transition, then this will be limited to the max energy usable by the phase transition.
			DecimalNumber phaseTransitioningInternalEnergy = DecimalNumber.Min(unit.InternalEnergy - energyLevelAtStartOfPhaseTransition, energyLevelAtCompletePhaseTransition);
			// If there was more internal energy than necessary for just phase transitioning, this will contain that excess energy. Otherwise it will be 0.
			DecimalNumber internalEnergyBeyondThatWhichCouldPossiblyBeUsed = DecimalNumber.Max(unit.InternalEnergy - energyLevelAtCompletePhaseTransition, 0); //DecimalNumber.Max((unit.InternalEnergy - energyLevelAtCompletePhaseTransition) - phaseTransitioningInternalEnergy, 0);

			// Calculates the substance amount that will undergo phase transition, preventing transitioning too much by limiting it to the available substance amount.
			DecimalNumber substanceAmountThatIsPhaseTransitioned = DecimalNumber.Min(phaseTransitioningInternalEnergy / phaseTransitionEnthalpy, unit.Moles);
			// todo: can energy be lost because of the min()?

			Debug.Assert(substanceAmountThatIsPhaseTransitioned >= 0);

			ResourceUnitData phaseTransitionedUnit = new(targetType, substanceAmountThatIsPhaseTransitioned, 0);

			// The internal energy of the phase transitioned material is .......
			phaseTransitionedUnit.InternalEnergy = ((energyLevelAtStartOfPhaseTransition / unit.Moles) * phaseTransitionedUnit.Moles) + phaseTransitioningInternalEnergy + internalEnergyBeyondThatWhichCouldPossiblyBeUsed;

			resourceContainer.AddResource(phaseTransitionedUnit);

			resourceContainer.AddResource(ResourceUnitData.CreateNegativeResourceUnit(unit.ResourceType, -phaseTransitionedUnit.Moles, -phaseTransitionedUnit.InternalEnergy));

			Debug.Assert(resourceContainer.GetResourceUnitData(unit.ResourceType).InternalEnergy >= 0);
			Debug.Assert(resourceContainer.GetResourceUnitData(phaseTransitionedUnit.ResourceType).InternalEnergy >= 0);
		}
		else
		{
			// The internal energy level that would be requited to start the transition. Calculated by taking the internal energy level of the phase transition start.
			DecimalNumber energyLevelAtStartOfPhaseTransition = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass);
			// The internal energy level that would be required for a complete phase transition. Calculated by taking the internal energy level of the start of the transition and then adding the energy required for the entire transition.
			DecimalNumber energyLevelAtCompletePhaseTransition = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass) - phaseTransitionEnthalpy * unit.Moles;

			// The amount of energy that is used for the phase transition. If there is more internal energy than is needed for a full phase transition, then this will be limited to the max energy usable by the phase transition.
			DecimalNumber phaseTransitioningInternalEnergy;

			if (unit.InternalEnergy < energyLevelAtStartOfPhaseTransition)
			{
				if (unit.InternalEnergy > energyLevelAtCompletePhaseTransition)
				{
					phaseTransitioningInternalEnergy = energyLevelAtStartOfPhaseTransition - unit.InternalEnergy;
				}
				else
				{
					phaseTransitioningInternalEnergy = energyLevelAtCompletePhaseTransition - unit.InternalEnergy;
				}
			}
			else
			{
				throw new GlitchInTheMatrixException("Should never have entered a phase transition!");
			}

			// If there was more internal energy than necessary for just phase transitioning, this will contain that excess energy. Otherwise it will be 0.
			DecimalNumber internalEnergyBelowThatWhichCouldPossiblyBeUsed;

			if (unit.InternalEnergy < energyLevelAtCompletePhaseTransition)
			{
				internalEnergyBelowThatWhichCouldPossiblyBeUsed = energyLevelAtCompletePhaseTransition - unit.InternalEnergy;
			}
			else
			{
				internalEnergyBelowThatWhichCouldPossiblyBeUsed = 0;
			}

			// Calculates the substance amount that will undergo phase transition, preventing transitioning too much by limiting it to the available substance amount.
			DecimalNumber substanceAmountThatIsPhaseTransitioned = DecimalNumber.Min(phaseTransitioningInternalEnergy / phaseTransitionEnthalpy, unit.Moles);
			// todo: can energy be lost because of the min()?

			Debug.Assert(substanceAmountThatIsPhaseTransitioned >= 0);

			ResourceUnitData phaseTransitionedUnit = new(targetType, substanceAmountThatIsPhaseTransitioned, 0);

			// The internal energy of the phase transitioned material is .......
			phaseTransitionedUnit.InternalEnergy = ((energyLevelAtStartOfPhaseTransition / unit.Moles) * phaseTransitionedUnit.Moles) + phaseTransitioningInternalEnergy - internalEnergyBelowThatWhichCouldPossiblyBeUsed;

			resourceContainer.AddResource(phaseTransitionedUnit);

			resourceContainer.AddResource(ResourceUnitData.CreateNegativeResourceUnit(unit.ResourceType, -phaseTransitionedUnit.Moles, -phaseTransitionedUnit.InternalEnergy));

			Debug.Assert(resourceContainer.GetResourceUnitData(unit.ResourceType).InternalEnergy >= 0);
			Debug.Assert(resourceContainer.GetResourceUnitData(phaseTransitionedUnit.ResourceType).InternalEnergy >= 0);
		}
	}
}
