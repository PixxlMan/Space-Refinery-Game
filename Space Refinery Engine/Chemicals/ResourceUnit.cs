﻿using ImGuiNET;
using System.Diagnostics;

namespace Space_Refinery_Engine
{
	public class ResourceUnit : IUIInspectable, IEquatable<ResourceUnit>
	{
		public ResourceType ResourceType { get; private set; }

		public ResourceContainer ResourceContainer { get; private set; }

		public ChemicalType ChemicalType => ResourceType.ChemicalType;

		private MolesUnit moles;

		/// <summary>
		/// Substance amount in [mol].
		/// </summary>
		public MolesUnit Moles
		{
			get
			{
				lock (syncRoot)
					return moles;
			}

			private set
			{
				Debug.Assert(value >= 0, "The number of moles cannot be less than zero.");

				lock (syncRoot)
				{
					moles = value;
				}

				ResourceUnitChanged?.Invoke(this);
			}
		}

		// TODO: add property or method in ChemicalType that gives absolute internal energy. maybe internal energy should be renamed to phase energy?

		private EnergyUnit internalEnergy;

		/// <summary>
		/// [J] Internal energy in the current phase.
		/// </summary>
		public EnergyUnit InternalEnergy
		{
			get
			{
				lock (syncRoot)
					return internalEnergy;
			}

			private set
			{
				//Debug.Assert(value >= 0, "Internal energy cannot be less than zero.");
				// Disable this assert since being able to create resources with negative internalEnergy might be useful for removing energy. Will have to see if this should be reimplemented in some other way.

				lock (syncRoot)
				{
					internalEnergy = value;
				}

				ResourceUnitChanged?.Invoke(this);
			}
		}

		/// <summary>
		/// [K]
		/// </summary>
		/// <remarks>
		/// If the substance amount or the mass is zero, the temperature will be considered to be zero.
		/// </remarks>
		public TemperatureUnit Temperature =>
			Moles != 0 && Mass != 0
				? ChemicalType.InternalEnergyToTemperature(ResourceType, InternalEnergy, Mass)
				: 0;

		/// <summary>
		/// Mass in [kg].
		/// </summary>
		public MassUnit Mass => ChemicalType.MolesToMass(ChemicalType, Moles);

		/// <summary>
		/// Volume in [m³].
		/// </summary>
		public VolumeUnit Volume => Mass / ResourceType.Density;

		/// <summary>
		/// The amount of energy [J] per unit of substance [mol] [J/mol]
		/// </summary>
		public MolarEnergyUnit MolarEnergy => InternalEnergy / Moles;

		public event Action<ResourceUnit> ResourceUnitChanged;

		public ResourceUnitData ResourceUnitData => new(ResourceType, Moles, InternalEnergy); // Not using CreateNegativeResourceUnit here is fine because while ResourceUnitData can be allowed to be negative, a ResourceUnit should always represent a real resource amount.

		private object syncRoot = new();

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
				ImGui.Text($"{nameof(MolarEnergy)}: {MolarEnergy.FormatMolarEnergy()}");
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