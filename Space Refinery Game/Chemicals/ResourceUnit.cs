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
	public struct ResourceUnit : IUIInspectable, IEquatable<ResourceUnit>
	{
		public ResourceType ResourceType;

		public readonly ChemicalType ChemicalType => ResourceType.ChemicalType;

		public DecimalNumber Mass; // kg

		public DecimalNumber Volume => (DecimalNumber)Mass / ResourceType.Density; // m3

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

		public ResourceUnit(ResourceType resourceType, DecimalNumber mass, DecimalNumber internalEnergy, DecimalNumber pressure)
		{
			ResourceType = resourceType;
			Mass = mass;
			InternalEnergy = internalEnergy;
			//Pressure = pressure;
		}

		public static ResourceUnit Add(ResourceUnit a, ResourceUnit b)
		{
			if (a.ResourceType != b.ResourceType)
			{
				throw new InvalidOperationException($"The {nameof(ResourceType)} of both {nameof(a)} and {nameof(b)} was not the same.");
			}

			ResourceUnit resourceUnit = new()
			{
				ResourceType = a.ResourceType,
				Mass = a.Mass + b.Mass,
				InternalEnergy = a.InternalEnergy + b.InternalEnergy,
				//Pressure = a.Pressure + b.Pressure, // ?
			};

			return resourceUnit;
		}

		public static void DoCreation(ChemicalType selected, ref ResourceUnit newResourceUnit)
		{
			ImGui.Indent();
			{
				newResourceUnit.ResourceType = selected.LiquidPhaseType;

				float mass = newResourceUnit.Mass.ToFloat();
				ImGui.SliderFloat("Mass (kg)", ref mass, 0, 100);
				newResourceUnit.Mass = DecimalNumber.FromFloat(mass);

				ImGui.Text($"Volume: {newResourceUnit.Volume} m3");
			}
			ImGui.Unindent();
		}

		public static ResourceUnit Remove(ResourceUnit a, ResourceUnit b)
		{
			if (a.ResourceType != b.ResourceType)
			{
				throw new InvalidOperationException($"The {nameof(ResourceType)} of both {nameof(a)} and {nameof(b)} was not the same.");
			}

			ResourceUnit resourceUnit = new()
			{
				ResourceType = a.ResourceType,
				Mass = a.Mass - b.Mass,
				InternalEnergy = a.InternalEnergy - b.InternalEnergy,
				//Pressure = a.Pressure - b.Pressure, // ?
			};

			return resourceUnit;
		}

		public override bool Equals(object? obj)
		{
			return obj is ResourceUnit unit && Equals(unit);
		}

		public bool Equals(ResourceUnit other)
		{
			return EqualityComparer<ResourceType>.Default.Equals(ResourceType, other.ResourceType) &&
				   Mass.Equals(other.Mass) &&
				   InternalEnergy.Equals(other.InternalEnergy) &&
				   Pressure.Equals(other.Pressure);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ResourceType, Mass, InternalEnergy, Pressure);
		}

		public static ResourceUnit Part(ResourceUnit unit, DecimalNumber part)
		{
			ResourceUnit resourceUnit = new()
			{
				ResourceType = unit.ResourceType,
				Mass = unit.Mass * part,
				InternalEnergy = unit.InternalEnergy * part,
				//Pressure = unit.Pressure * transferPart, // ?
			};

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

				ImGui.Text($"Mass: {Mass} kg");
				ImGui.Text($"Volume: {Volume} m3");
				ImGui.Text($"Internal Energy: {InternalEnergy} ?");
				ImGui.Text($"Enthalpy: {Enthalpy} ?");
				ImGui.Text($"Temperature: {Temperature} K");
				ImGui.Text($"Pressure: {Pressure} Pa");
			}
			UIFunctions.EndSub();
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

		public static ResourceUnit operator +(ResourceUnit left, ResourceUnit right)
		{
			return Add(left, right);
		}

		public static ResourceUnit operator -(ResourceUnit left, ResourceUnit right)
		{
			return Remove(left, right);
		}

		public void Serialize(XmlWriter writer)
		{
			writer.WriteElementString(nameof(ChemicalType.ChemicalName), ChemicalType.ChemicalName);
			writer.WriteElementString(nameof(ResourceType.ChemicalPhase), ResourceType.ChemicalPhase.ToString());
			writer.Serialize(Mass, nameof(Mass));
			writer.Serialize(InternalEnergy, nameof(InternalEnergy));
			writer.Serialize(Pressure, nameof(Pressure));
		}

		public static ResourceUnit Deserialize(XmlReader reader)
		{
			ChemicalType chemicalType;
			ResourceType resourceType;
			DecimalNumber mass;
			DecimalNumber internalEnergy;
			DecimalNumber pressure;

			reader.ReadStartElement(nameof(Space_Refinery_Game.ChemicalType.ChemicalName));
			{
				chemicalType = MainGame.ChemicalTypesDictionary[reader.ReadContentAsString()];
			}
			reader.ReadEndElement();
			reader.ReadStartElement(nameof(Space_Refinery_Game.ResourceType.ChemicalPhase));
			{
				ChemicalPhase chemicalPhase = Enum.Parse<ChemicalPhase>(reader.ReadContentAsString());

				resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);
			}
			reader.ReadEndElement();
			mass = reader.DeserializeDecimalNumber(nameof(Mass));
			internalEnergy = reader.DeserializeDecimalNumber(nameof(InternalEnergy));
			pressure = reader.DeserializeDecimalNumber(nameof(Pressure));

			return new(resourceType, mass, internalEnergy, pressure);
		}
	}
}
