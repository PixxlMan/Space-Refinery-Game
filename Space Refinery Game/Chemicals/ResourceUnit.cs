using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

namespace Space_Refinery_Game
{
	public class ResourceUnit : IUIInspectable, IEquatable<ResourceUnit>
	{
		public ResourceType ResourceType { get; private set; }

		public ResourceContainer ResourceContainer { get; private set; }

		public ChemicalType ChemicalType => ResourceType.ChemicalType;

		private DecimalNumber moles;

		/// <summary>
		/// [mol]
		/// </summary>
		public DecimalNumber Moles
		{
			get
			{
				lock (syncRoot)
					return moles;
			}

			private set
			{
				Debug.Assert(value >= 0, "The number of moles cannot be less than zero.");

				ResourceUnitChanged?.Invoke(this);

				lock (syncRoot)
				{
					moles = value;
				}
			}
		}

		private DecimalNumber internalEnergy;

		/// <summary>
		/// Internal energy in [J].
		/// </summary>
		public DecimalNumber InternalEnergy
		{
			get
			{
				lock (syncRoot)
					return internalEnergy;
			}

			private set
			{
				Debug.Assert(value >= 0, "Internal energy cannot be less than zero.");

				ResourceUnitChanged?.Invoke(this);

				lock (syncRoot)
				{
					internalEnergy = value;
				}
			}
		}

		public DecimalNumber Temperature => ChemicalType.InternalEnergyToTemperature(ResourceType, InternalEnergy, Mass);

		private object syncRoot = new();

		public DecimalNumber Mass => ChemicalType.MolesToMass(ChemicalType, Moles); // [kg]

		public DecimalNumber Volume => Mass / ResourceType.Density; // [m³]

		public event Action<ResourceUnit> ResourceUnitChanged;

		public ResourceUnitData ResourceUnitData => new(ResourceType, Moles, InternalEnergy);

		public ResourceUnit(ResourceType resourceType, ResourceContainer resourceContainer, ResourceUnitData resourceUnitData)
		{
			ResourceType = resourceType;
			ResourceContainer = resourceContainer;
			moles = resourceUnitData.Moles;
			internalEnergy = resourceUnitData.InternalEnergy;
		}

		public override bool Equals(object? obj)
		{
			return obj is ResourceUnit unit && Equals(unit);
		}

		public bool Equals(ResourceUnit other)
		{
			return ReferenceEquals(ResourceType, other.ResourceType) &&
				   Moles.Equals(other.Moles) &&
				   InternalEnergy.Equals(other.internalEnergy);
		}

		public void Add(ResourceUnitData resourceUnitData)
		{
			if (!ReferenceEquals(resourceUnitData.ResourceType, ResourceType))
			{
				throw new ArgumentException($"The {nameof(ResourceUnitData)}'s ResourceType is different from this {nameof(ResourceUnit)}'s ResourceType.", $"{nameof(resourceUnitData)}");
			}

			Moles += resourceUnitData.Moles;
			InternalEnergy += resourceUnitData.InternalEnergy;
		}

		public void Remove(ResourceUnitData resourceUnitData)
		{
			if (!ReferenceEquals(resourceUnitData.ResourceType, ResourceType))
			{
				throw new ArgumentException($"The {nameof(ResourceUnitData)}'s ResourceType is different from this {nameof(ResourceUnit)}'s ResourceType.", $"{nameof(resourceUnitData)}");
			}

			Moles -= resourceUnitData.Moles;
			InternalEnergy -= resourceUnitData.InternalEnergy;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ResourceType, Moles);
		}

		public void DoUIInspectorReadonly()
		{
			UIFunctions.BeginSub();
			{
				ImGui.Text($"Resource type: {ResourceType.ResourceName}");

				ImGui.Text($"{nameof(Moles)}: {Moles.FormatSubstanceAmount()}");
				ImGui.Text($"{nameof(Mass)}: {Mass.FormatMass()}");
				ImGui.Text($"{nameof(Volume)}: {Volume.FormatVolume()}");
				ImGui.Text($"{nameof(InternalEnergy)}: {InternalEnergy.FormatEnergy()}");
				ImGui.Text($"{nameof(Temperature)}: {Temperature.FormatTemperature()}");
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
	}
}
