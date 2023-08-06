using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public struct ResourceUnitData : IUIInspectable, IEquatable<ResourceUnit>
	{
		// Fields

		public ResourceType ResourceType;

		public ChemicalType ChemicalType => ResourceType.ChemicalType;

		// Properties

		/// <summary>
		/// [mol]
		/// </summary>
		public MolesUnit Moles;

		/// <summary>
		/// [J] Internal energy in the current phase.
		/// </summary>
		public EnergyUnit InternalEnergy;

		// Driven properties

		/// <summary>
		/// [kg]
		/// </summary>
		public MassUnit Mass => ChemicalType.MolesToMass(ChemicalType, Moles);

		/// <summary>
		/// [m³]
		/// </summary>
		public VolumeUnit Volume => Mass / ResourceType.Density;

		/// <summary>
		/// [K]
		/// </summary>
		/// <remarks>
		/// If the substance amount or the mass is zero, the temperature will be considered to be zero.
		/// </remarks>
		public TemperatureUnit Temperature => ((DecimalNumber)Moles != 0 && (DecimalNumber)Mass != 0) ? ChemicalType.InternalEnergyToTemperature(ResourceType, InternalEnergy, Mass) : 0;

		/// <summary>
		/// [J/mol]
		/// </summary>
		public MolarEnergyUnit MolarEnergy => InternalEnergy / Moles;

		// Methods

		public void BreakInto(int currentSubstanceAmountRatio, out IReadOnlyDictionary<ResourceType, ResourceUnitData> resourceUnitDatas, params (ResourceType resourceType, int substanceAmountRatio)[] resourceAndSMRs)
		{
			Dictionary<ResourceType, ResourceUnitData> resourceUnitDataDictionary = new();

			foreach (var (resourceType, SMR) in resourceAndSMRs)
			{
				resourceUnitDataDictionary.Add(
					resourceType,
						new(
							resourceType,
							(MolesUnit)((DecimalNumber)(Moles * SMR) / currentSubstanceAmountRatio), // Distribute moles according to SMR and substance amount ratio (Sorry, I don't know what that actually means either - it was like this when I came! Promise.).
							(EnergyUnit)((DecimalNumber)InternalEnergy / resourceAndSMRs.Length))); // Evenly distribute energy.
			}

			resourceUnitDatas = resourceUnitDataDictionary;
		}

		public ResourceUnitData(ResourceType resourceType, MolesUnit moles, EnergyUnit internalEnergy)
		{
			Debug.Assert(resourceType is not null, $"Argument {nameof(resourceType)} should never be null.");

			Debug.Assert(moles >= 0, "The number of moles cannot be lower than zero.");

			Debug.Assert(internalEnergy >= 0 || resourceType.ChemicalPhase != ChemicalPhase.Solid, "Internal energy when the chemical phase is solid cannot be lower than zero.");

			ResourceType = resourceType;

			Moles = moles;

			InternalEnergy = internalEnergy;
		}

		/// <summary>
		/// Works like the regular constructor, however it doesn't contain checks therefore allowing a negative amount of moles or internal energy.
		/// This can be useful for using <c>ResourceUnit</c>s for subtracting resources.
		/// </summary>
		public static ResourceUnitData CreateNegativeResourceUnit(ResourceType resourceType, MolesUnit moles, EnergyUnit internalEnergy)
		{
			Debug.Assert(resourceType is not null, $"Argument {nameof(resourceType)} should never be null.");

			return new()
			{
				ResourceType = resourceType,
				Moles = moles,
				InternalEnergy = internalEnergy,
			};
		}

		public void DoUIInspectorReadonly() // Why is this different from ResourceUnit.DoUIInspectorReadonly? Should ResourceUnit.DoUIInspectorReadonly() refer to this instead, updating this as well?
		{
			throw new NotImplementedException();

			UIFunctions.BeginSub();
			{
				if (ImGui.CollapsingHeader($"Resource type"))
				{
					ResourceType.DoUIInspectorReadonly();
				}

				ImGui.Text($"Moles: {Moles.FormatSubstanceAmount()}");
				ImGui.Text($"Mass: {Mass.FormatMass()}");
				ImGui.Text($"Volume: {Volume.FormatVolume()}");
			}
			UIFunctions.EndSub();
		}

		public IUIInspectable DoUIInspectorEditable()
		{
			throw new NotImplementedException();
		}

		public static void DoCreation(ChemicalType selected, ref ResourceUnitData newResourceUnit)
		{
			ImGui.Indent();
			{
				newResourceUnit.ResourceType = selected.GetResourceTypeForPhase(selected.CommonPhase);

				float massFloat = ((DecimalNumber)newResourceUnit.Mass).ToFloat();
				ImGui.SliderFloat("Mass (kg)", ref massFloat, 0, 100);
				newResourceUnit.Moles = ChemicalType.MassToMoles(selected, (MassUnit)(DN)massFloat);

				ImGui.Text($"Moles: {newResourceUnit.Moles.FormatSubstanceAmount()}");

				ImGui.Text($"Volume: {newResourceUnit.Volume.FormatVolume()}");

				// Temperature here...
			}
			ImGui.Unindent();
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ResourceType, Moles);
		}

		public void Serialize(XmlWriter writer)
		{
			writer.WriteElementString(nameof(ChemicalType.ChemicalName), ChemicalType.ChemicalName);
			writer.Serialize(ResourceType.ChemicalPhase.ToString(), nameof(ChemicalPhase));
			writer.Serialize(Moles, nameof(Moles));
			writer.Serialize(InternalEnergy, nameof(InternalEnergy));
		}

		public static ResourceUnitData Deserialize(XmlReader reader)
		{
			ChemicalType chemicalType;
			ResourceType resourceType;
			MolesUnit moles;
			EnergyUnit internalEnergy;

			chemicalType = ChemicalType.GetChemicalType(reader.ReadString(nameof(Space_Refinery_Game.ChemicalType.ChemicalName)));
			resourceType = chemicalType.GetResourceTypeForPhase(reader.DeserializeEnum<ChemicalPhase>(nameof(ChemicalPhase)));
			moles = reader.DeserializeUnit<MolesUnit>(nameof(Moles));
			internalEnergy = reader.DeserializeUnit<EnergyUnit>(nameof(InternalEnergy));

			return new(resourceType, moles, internalEnergy);
		}

		public override bool Equals([NotNullWhen(true)] object obj)
		{
			return obj is ResourceUnitData
				&& Equals((ResourceUnitData)obj);
		}

		public bool Equals(ResourceUnit other)
		{
			return other.ResourceType == ResourceType
				&& other.Moles == Moles
				&& other.InternalEnergy == InternalEnergy;
		}
	}
}
