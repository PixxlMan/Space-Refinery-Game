﻿using ImGuiNET;
using System.Diagnostics;

namespace Space_Refinery_Engine;

// TODO: ResourceUnit should just refer to an internal ResourceUnitData instead of having own fields here
public sealed class ResourceUnit : IUIInspectable, IEquatable<ResourceUnit>
{
	public ResourceType ResourceType { get; private set; }

	public ChemicalType ChemicalType => ResourceType.ChemicalType;

	private MolesUnit moles;
	/// <summary>
	/// [mol] Substance amount.
	/// </summary>
	public MolesUnit Moles
	{
		get
		{
			lock (syncRoot)
			{
				return moles;
			}
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

	// TODO: rename to TemperatureEnergy
	private EnergyUnit internalEnergy;
	/// <summary>
	/// [J] Internal energy directly affecting the temperature.
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
	/// [K] The temperature of the resource. Depends on the specific heat capacity of the material and the internal energy.
	/// </summary>
	/// <remarks>
	/// If the substance amount or the mass is zero, the temperature will be considered to be zero
	/// </remarks>
	public TemperatureUnit Temperature
	{
		get
		{
			if (Moles != 0 && Mass != 0)
			{
				return ChemicalType.InternalEnergyToTemperature(ResourceType, InternalEnergy, Mass);
			}
			else
			{
				return 0;
			}
		}
	}

	/// <summary>
	/// [kg] Mass in kilograms.
	/// </summary>
	public MassUnit Mass => ChemicalType.MolesToMass(ChemicalType, Moles);

	/// <summary>
	/// [m³] The volume of a non compressable resource in cubic meters.
	/// Compressable resources always have zero non compressable volume.
	/// </summary>
	public VolumeUnit NonCompressableVolume
	{
		get
		{
			if (!ResourceType.Compressable)
			{
				return Mass / ResourceType.Density;
			}
			else
			{ // Will compress to fill remaining available space.
				return 0;
			}
		}
	}

	public VolumeUnit UncompressedVolume
	{
		get
		{
			return Mass / ResourceType.Density;
		}
	}

	/// <summary>
	/// The amount of energy [J] per unit of substance [mol] [J/mol]
	/// </summary>
	public MolarEnergyUnit MolarEnergy => InternalEnergy / Moles;

	public event Action<ResourceUnit>? ResourceUnitChanged;

	public ResourceUnitData ResourceUnitData => new(ResourceType, Moles, InternalEnergy); // Not using CreateNegativeResourceUnit here is fine because while ResourceUnitData can be allowed to be negative, a ResourceUnit should always represent a real resource amount.

	private object syncRoot = new();

	public ResourceUnit(ResourceType resourceType, ResourceUnitData resourceUnitData)
	{
		ResourceType = resourceType;
		moles = resourceUnitData.Moles;
		internalEnergy = resourceUnitData.InternalEnergy;
	}

	public override bool Equals(object? obj)
	{
		return obj is ResourceUnit unit && Equals(unit);
	}

	public bool Equals(ResourceUnit? other)
	{
		return ReferenceEquals(ResourceType, other?.ResourceType) &&
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
			ImGui.Text($"{nameof(NonCompressableVolume)}: {NonCompressableVolume.FormatVolume()}");
			ImGui.Text($"{nameof(UncompressedVolume)}: {UncompressedVolume.FormatVolume()}");
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
