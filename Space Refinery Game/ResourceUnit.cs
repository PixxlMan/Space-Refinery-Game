using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public struct ResourceUnit : IUIInspectable, IEquatable<ResourceUnit>
	{
		public ResourceType ResourceType;

		public readonly ChemicalType ChemicalType => ResourceType.ChemicalType;

		public FixedDecimalInt4 Mass; // kg

		public FixedDecimalLong8 Volume => (FixedDecimalLong8)Mass / ResourceType.Density; // m3

		public FixedDecimalInt4 InternalEnergy; // kJ

		public FixedDecimalInt4 Temperature // k
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

		public FixedDecimalInt4 Pressure => /*temporarily set to 101325 Pa*/ 101325; // Pa or kg/m3

		public FixedDecimalInt4 Enthalpy => InternalEnergy + Pressure * (FixedDecimalInt4)Volume; // https://www.omnicalculator.com/physics/enthalpy

		public ResourceUnit(ResourceType resourceType, FixedDecimalInt4 mass, FixedDecimalInt4 internalEnergy, FixedDecimalInt4 pressure)
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
				newResourceUnit.Mass = FixedDecimalInt4.FromFloat(mass);

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

		public static ResourceUnit Part(ResourceUnit unit, FixedDecimalInt4 transferPart)
		{
			ResourceUnit resourceUnit = new()
			{
				ResourceType = unit.ResourceType,
				Mass = unit.Mass * transferPart,
				InternalEnergy = unit.InternalEnergy * transferPart,
				//Pressure = unit.Pressure * transferPart, // ?
			};

			return resourceUnit;
		}

		public void DoUIInspectorReadonly()
		{
			throw new NotImplementedException();
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
	}
}
