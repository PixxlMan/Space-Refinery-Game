using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public class ResourceUnit : IUIInspectable, IEquatable<ResourceUnit>
	{
		public ResourceType ResourceType;

		public ChemicalType ChemicalType => ResourceType.ChemicalType;

		public DecimalNumber Moles;

		public DecimalNumber Mass => ChemicalType.MolesToMass(ChemicalType, Moles); // kg

		public DecimalNumber Volume => Mass / ResourceType.Density; // m3

		public DecimalNumber InternalEnergy; // kJ

		public DecimalNumber Temperature // k
		{
			get
			{
				if (Mass == 0)
				{
					throw new InvalidOperationException("Temperature of an object with no mass is undefined.");
				}

				return InternalEnergy / (Mass * ResourceType.SpecificHeatCapacity);
			}
		}

		public DecimalNumber Pressure => /*temporarily set to 101325 Pa*/ 101325; // Pa or kg/m3

		public DecimalNumber Enthalpy => InternalEnergy + Pressure * Volume; // https://www.omnicalculator.com/physics/enthalpy

		public ResourceUnit(ResourceType resourceType, DecimalNumber moles, DecimalNumber internalEnergy)
		{
			ResourceType = resourceType;
			Moles = moles;
			InternalEnergy = internalEnergy;
			//Pressure = pressure;
		}

		public static void DoCreation(ChemicalType selected, ref ResourceUnit newResourceUnit)
		{
			ImGui.Indent();
			{
				newResourceUnit.ResourceType = selected.LiquidPhaseType;

				float mass = newResourceUnit.Mass.ToFloat();
				ImGui.SliderFloat("Mass (kg)", ref mass, 0, 100);
				newResourceUnit.Moles = ChemicalType.MassToMoles(selected, DecimalNumber.FromFloat(mass));

				ImGui.Text($"Moles: {newResourceUnit.Moles} mol");

				ImGui.Text($"Volume: {newResourceUnit.Volume} m3");
			}
			ImGui.Unindent();
		}

		public ResourceUnit Clone()
		{
			return new(ResourceType, Moles, InternalEnergy);
		}

		public override bool Equals(object? obj)
		{
			return obj is ResourceUnit unit && Equals(unit);
		}

		public bool Equals(ResourceUnit other)
		{
			return EqualityComparer<ResourceType>.Default.Equals(ResourceType, other.ResourceType) &&
				   Moles.Equals(other.Moles) &&
				   InternalEnergy.Equals(other.InternalEnergy) &&
				   Pressure.Equals(other.Pressure);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ResourceType, Moles, InternalEnergy, Pressure);
		}

		public static ResourceUnit GetPart(ResourceUnit unit, DecimalNumber part)
		{
			ResourceUnit resourceUnit = new(unit.ResourceType, unit.Moles * part, unit.InternalEnergy * part);

			return resourceUnit;
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
				ImGui.Text($"Volume: {Volume} m3");
				ImGui.Text($"Internal Energy: {InternalEnergy} ?");
				ImGui.Text($"Enthalpy: {Enthalpy} ?");
				ImGui.Text($"Temperature: {Temperature} K");
				ImGui.Text($"Pressure: {Pressure} Pa");
			}
			UIFunctions.EndSub();
		}

		public void Remove(ResourceUnit b)
		{
			if (b.ResourceType != ResourceType)
			{
				throw new ArgumentException($"The other {nameof(ResourceUnit)} had another {nameof(ResourceType)}.", nameof(b));
			}

			Moles -= b.Moles;
			InternalEnergy -= b.InternalEnergy;
		}

		public void Add(ResourceUnit b)
		{
			if (b.ResourceType != ResourceType)
			{
				throw new ArgumentException($"The other {nameof(ResourceUnit)} had another {nameof(ResourceType)}.", nameof(b));
			}

			Moles += b.Moles;
			InternalEnergy += b.InternalEnergy;
		}

		public IUIInspectable DoUIInspectorEditable()
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(ResourceUnit left, ResourceUnit right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ResourceUnit left, ResourceUnit right)
		{
			return !(left == right);
		}

		public void Serialize(XmlWriter writer)
		{
			writer.WriteElementString(nameof(ChemicalType.ChemicalName), ChemicalType.ChemicalName);
			writer.Serialize(ResourceType.ChemicalPhase.ToString(), nameof(ChemicalPhase));
			writer.Serialize(Moles, nameof(Moles));
			writer.Serialize(InternalEnergy, nameof(InternalEnergy));
			//writer.Serialize(Pressure, nameof(Pressure));
		}

		public static ResourceUnit Deserialize(XmlReader reader)
		{
			ChemicalType chemicalType;
			ResourceType resourceType;
			DecimalNumber moles;
			DecimalNumber internalEnergy;
			DecimalNumber pressure;

			chemicalType = MainGame.ChemicalTypesDictionary[reader.ReadString(nameof(Space_Refinery_Game.ChemicalType.ChemicalName))];
			reader.ReadEndElement();
			resourceType = chemicalType.GetResourceTypeForPhase(reader.DeserializeEnum<ChemicalPhase>(nameof(ChemicalPhase)));
			moles = reader.DeserializeDecimalNumber(nameof(Moles));
			internalEnergy = reader.DeserializeDecimalNumber(nameof(InternalEnergy));
			//pressure = reader.DeserializeDecimalNumber(nameof(Pressure));

			return new(resourceType, moles, internalEnergy);
		}
	}
}
