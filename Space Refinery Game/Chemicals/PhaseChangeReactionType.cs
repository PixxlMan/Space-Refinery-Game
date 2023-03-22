using Space_Refinery_Utilities;

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
							Console.WriteLine("Melting");
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
							Console.WriteLine("Vaporizing");
						}
						else if (unit.Temperature < type.ChemicalType.TemperatureOfFusion)
						{
							TransitionPhase(resourceContainer, unit, ChemicalPhase.Solid, chemicalType.TemperatureOfFusion, chemicalType.EnthalpyOfFusion);
							producedReactionFactors.Add(new Solidifying());
							Console.WriteLine("Solidifying");
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
							Console.WriteLine("Condensating");
						}
						break;
					default:
						throw new GlitchInTheMatrixException();
				}
			}
		}
	}

	private static void TransitionPhase(ResourceContainer resourceContainer, ResourceUnitData unit, ChemicalPhase targetPhase, DecimalNumber temperatureOfPhaseTransition, DecimalNumber enthalpy)
	{
		ResourceType type = unit.ResourceType;
		ChemicalType chemicalType = type.ChemicalType;

		if (targetPhase > type.ChemicalPhase) // This is a transition to a greater state of entropy.
		{
			DecimalNumber energyForCompletePhaseTransition = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass) + enthalpy * unit.Mass;
			DecimalNumber energyForPhaseTransitionStart = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass);
			DecimalNumber excessEnergy = unit.InternalEnergy - energyForPhaseTransitionStart;
			DecimalNumber massAffected = excessEnergy / enthalpy;
			DecimalNumber molsAffected = DecimalNumber.Min(ChemicalType.MassToMoles(chemicalType, massAffected), unit.Moles);

			DecimalNumber transitionEnergySurplus = DecimalNumber.Max(unit.InternalEnergy - energyForCompletePhaseTransition, 0);

			var moltenUnit = resourceContainer.TakeResourceByMoles(type, molsAffected);
			moltenUnit.ResourceType = chemicalType.GetResourceTypeForPhase(targetPhase);
			moltenUnit.InternalEnergy = transitionEnergySurplus + ChemicalType.TemperatureToInternalEnergy(moltenUnit.ResourceType, temperatureOfPhaseTransition, moltenUnit.Mass);
			resourceContainer.AddResource(moltenUnit);
		}
		else // This is a transition to a lower state of entropy.
		{
			DecimalNumber energyForCompletePhaseTransition = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass) + enthalpy * unit.Mass;
			DecimalNumber energyForPhaseTransitionStart = ChemicalType.TemperatureToInternalEnergy(type, temperatureOfPhaseTransition, unit.Mass);
			DecimalNumber excessEnergy = unit.InternalEnergy - energyForPhaseTransitionStart;
			DecimalNumber massAffected = excessEnergy / enthalpy;
			DecimalNumber molsAffected = DecimalNumber.Min(ChemicalType.MassToMoles(chemicalType, massAffected), unit.Moles);

			DecimalNumber transitionEnergySurplus = DecimalNumber.Max(unit.InternalEnergy - energyForCompletePhaseTransition, 0);

			var moltenUnit = resourceContainer.TakeResourceByMoles(type, molsAffected);
			moltenUnit.ResourceType = chemicalType.GetResourceTypeForPhase(targetPhase);
			moltenUnit.InternalEnergy = transitionEnergySurplus + ChemicalType.TemperatureToInternalEnergy(moltenUnit.ResourceType, temperatureOfPhaseTransition, moltenUnit.Mass);
			resourceContainer.AddResource(moltenUnit);
		}
	}
}
