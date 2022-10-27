using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public struct ResourceUnitData : IUIInspectable, IEquatable<ResourceUnit>
	{
		public ResourceType ResourceType;

		public ChemicalType ChemicalType => ResourceType.ChemicalType;

		/// <summary>
		/// [mol]
		/// </summary>
		public DecimalNumber Moles;

		/// <summary>
		/// [kg]
		/// </summary>
		public DecimalNumber Mass => ChemicalType.MolesToMass(ChemicalType, Moles);

		/// <summary>
		/// [m³]
		/// </summary>
		public DecimalNumber Volume => Mass / ResourceType.Density;

		public void BreakInto(int currentSubstanceAmountRatio, out IReadOnlyDictionary<ResourceType, ResourceUnitData> resourceUnitDatas, params (ResourceType resourceType, int substanceAmountRatio)[] resourceAndSMRs)
		{
			Dictionary<ResourceType, ResourceUnitData> resourceUnitDataDictionary = new();

			foreach (var (resourceType, SMR) in resourceAndSMRs)
			{
				resourceUnitDataDictionary.Add(resourceType, new(resourceType, (Moles * SMR) / currentSubstanceAmountRatio));
			}

			resourceUnitDatas = resourceUnitDataDictionary;
		}

		public ResourceUnitData(ResourceType resourceType, DecimalNumber moles)
		{
			ResourceType = resourceType;

			Moles = moles;

			Debug.Assert(resourceType is not null, $"Argument {nameof(resourceType)} should never be null.");

			Debug.Assert(moles >= 0, "The number of moles cannot be less than zero.");
		}

		public void DoUIInspectorReadonly()
		{
			UIFunctions.BeginSub();
			{
				if (ImGui.CollapsingHeader($"Resource type"))
				{
					ResourceType.DoUIInspectorReadonly();
				}

				ImGui.Text($"Moles: {Moles} mol");
				ImGui.Text($"Mass: {Mass} kg");
				ImGui.Text($"Volume: {Volume} m³");
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

				float mass = newResourceUnit.Mass.ToFloat();
				ImGui.SliderFloat("Mass (kg)", ref mass, 0, 100);
				newResourceUnit.Moles = ChemicalType.MassToMoles(selected, DecimalNumber.FromFloat(mass));

				ImGui.Text($"Moles: {newResourceUnit.Moles} mol");

				ImGui.Text($"Volume: {newResourceUnit.Volume} m³");
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
		}

		public static ResourceUnitData Deserialize(XmlReader reader)
		{
			ChemicalType chemicalType;
			ResourceType resourceType;
			DecimalNumber moles;

			chemicalType = ChemicalType.GetChemicalType(reader.ReadString(nameof(Space_Refinery_Game.ChemicalType.ChemicalName)));
			resourceType = chemicalType.GetResourceTypeForPhase(reader.DeserializeEnum<ChemicalPhase>(nameof(ChemicalPhase)));
			moles = reader.DeserializeDecimalNumber(nameof(Moles));

			return new(resourceType, moles);
		}

		public override bool Equals([NotNullWhen(true)] object obj)
		{
			return obj is ResourceUnitData
				&& Equals((ResourceUnitData)obj);
		}

		public bool Equals(ResourceUnit other)
		{
			return other.ResourceType == ResourceType
				&& other.Moles == Moles;
		}
	}
}
