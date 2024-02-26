using ImGuiNET;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class ResourceContainer : IUIInspectable // Thread safe? Seems like changes to resources can occur while reactions are taking place... Is that okay?
{
	private VolumeUnit volume;
	/// <summary>
	/// Volume in cubic meters [m³]
	/// </summary>
	public VolumeUnit Volume
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateVolume)
				{
					volume = RecalculateVolume();
				}

				return volume;
			}
		}
	}

	private VolumeUnit RecalculateVolume()
	{
		lock (SyncRoot)
		{
			recalculateVolume = false;

			VolumeUnit volume = 0;

			foreach (var resourceUnit in resources.Values)
			{
				volume += resourceUnit.Volume;
			}

			return volume;
		}
	}

	private MassUnit mass;
	/// <summary>
	/// Mass in kilograms [kg]
	/// </summary>
	public MassUnit Mass
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateMass)
				{
					mass = RecalculateMass();
				}

				return mass;
			}
		}
	}

	private MassUnit RecalculateMass()
	{
		lock (SyncRoot)
		{
			recalculateMass = false;

			MassUnit mass = 0;

			foreach (var resourceUnit in resources.Values)
			{
				mass += resourceUnit.Mass;
			}

			return mass;
		}
	}

	private VolumeUnit maxVolume;
	public VolumeUnit MaxVolume
	{
		get
		{
			lock (SyncRoot)
			{
				return maxVolume;
			}
		}
		set
		{
			Debug.Assert(value > 0, "Value cannot be negative.");
			lock (SyncRoot)
			{
				maxVolume = value;
			}
		}
	}

	public VolumeUnit FreeVolume => (MaxVolume - Volume).Max(0); // TODO: Rename Volume to OccupiedVolume? And MaxVolume to Volume?

	private PressureUnit pressure;
	/// <summary>
	/// [N/m²]
	/// </summary>
	public PressureUnit Pressure
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculatePressure)
				{
					pressure = RecalculatePressure();
				}

				return pressure;
			}
		}
	}

	private TemperatureUnit averageTemperature;
	/// <summary>
	/// [K] The average temperature of all resources in this container in kelvin
	/// </summary>
	public TemperatureUnit AverageTemperature
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateAverageTemperature)
				{
					averageTemperature = RecalculateAverageTemperature();
				}

				return averageTemperature;
			}
		}
	}

	private MolesUnit gasSubstanceAmount;
	/// <summary>
	/// The substance amount of all gas resources in this container in mols [mol]
	/// </summary>
	private MolesUnit GasSubstanceAmount
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateGasSubstanceAmount)
				{
					gasSubstanceAmount = RecalculateGasSubstanceAmount();
				}

				return gasSubstanceAmount;
			}
		}
	}

	private MolesUnit RecalculateGasSubstanceAmount()
	{
		lock (SyncRoot)
		{
			recalculateGasSubstanceAmount = false;

			MolesUnit totalSubstanceAmount = 0;

			foreach (var resourceUnit in resources.Values)
			{
				if (resourceUnit.ResourceType.ChemicalPhase == ChemicalPhase.Gas)
				{
					totalSubstanceAmount += resourceUnit.Moles;
				}
			}

			return (MolesUnit)((DN)totalSubstanceAmount / resources.Count);
		}
	}

	private PressureUnit RecalculatePressure()
	{
		lock (SyncRoot)
		{
			return Calculations.CalculatePressureUsingIdealGasLaw(GasSubstanceAmount, AverageTemperature, Volume /*- VolumeOfUncompressables*/);
		}
	}

	private TemperatureUnit RecalculateAverageTemperature()
	{
		lock (SyncRoot)
		{
			recalculateAverageTemperature = false;

			TemperatureUnit totalTemperature = 0;

			foreach (var resourceUnit in resources.Values)
			{
				totalTemperature += resourceUnit.Temperature;
			}

			return (TemperatureUnit)((DN)totalTemperature / resources.Count);
		}
	}

	bool recalculateVolume = false;

	bool recalculateMass = false;

	bool recalculatePressure = false;

	bool recalculateAverageTemperature = false;

	bool recalculateGasSubstanceAmount = false;

	public object SyncRoot = new();

	//public event Action ChemicalCompositionChanged

	public Portion<VolumeUnit> Fullness
	{
		get
		{
			lock (SyncRoot)
			{
				if ((DN)MaxVolume == 0)
				{
					return 1;
				}

				return Volume / MaxVolume;
			}
		}
	}

	private ConcurrentDictionary<ResourceType, ResourceUnit> resources = new();

	private static readonly VolumeUnit acceptableVolumeTransferError = 0.1;

	private static readonly DN permittedMaxPostReactionMassDiscrepancy = 0.001;

	public ResourceContainer(VolumeUnit maxVolume) : this()
	{
		this.maxVolume = maxVolume;
	}

	private ResourceContainer()
	{
		RecalculatePossibleReactionTypes();
	}

	public ResourceUnitData TakeResourceByMoles(ResourceType resourceType, MolesUnit moles)
	{
		if (moles == 0)
		{
			return new ResourceUnitData(resourceType, 0, 0);
		}

		ResourceUnit unit = resources[resourceType];

		Portion<MolesUnit> partTaken = moles / unit.Moles;
		EnergyUnit internalEnergyToTake = unit.InternalEnergy * (Portion<EnergyUnit>)(DN)partTaken;

		ResourceUnitData unitDataToTake = new(resourceType, moles, internalEnergyToTake);

		unit.Remove(unitDataToTake);

		return unitDataToTake;
	}

	public ResourceUnitData TakeAllResource(ResourceType resourceType)
	{
		ResourceUnit unit = resources[resourceType];

		ResourceUnitData takenResource = new(resourceType, unit.Moles, unit.InternalEnergy);

		unit.Remove(takenResource);

		return takenResource;
	}

	private ICollection<ReactionType> possibleReactionTypes;

	private bool invalidatePossibleReactionTypes = true;

	public void AddReactionFactor(ReactionFactor reactionFactor)
	{
		lock (producedReactionFactors)
		{
			producedReactionFactors.Add(reactionFactor);
		}
	}

	public void AddReactionFactors(IEnumerable<ReactionFactor> reactionFactors)
	{
		lock (producedReactionFactors)
		{
			producedReactionFactors.AddRange(reactionFactors);
		}
	}

	private ILookup<Type, ReactionFactor> reactionFactors;
	private readonly List<ReactionFactor> producedReactionFactors = new();

	public void Tick(IntervalUnit tickInterval)
	{
		reactionFactors = producedReactionFactors.ToLookup((rF) => rF.GetType());

		producedReactionFactors.Clear();

		/*#if DEBUG
					lock (SyncRoot) // lock because otherwise the assert to check the mass discrepency could Assert incorrectly because of external parallell modifications to the mass.
					{
						var initialMass = Mass;
		#endif*/
		foreach (var reactionType in possibleReactionTypes) // Tick all reactionTypes *before* recalculating possible reaction types - in order to ensure reactions can be stopped by noticing lack of resources. A reactionType should always be ticked normally before being removed from further ticks.
		{
			reactionType.Tick(tickInterval, this, reactionFactors, producedReactionFactors);
		}
		/*#if DEBUG
						Debug.Assert(DecimalNumber.Difference(initialMass, Mass) < permittedMaxPostReactionMassDiscrepancy);
					}
		#endif*/

		bool needsToRecalculatePossibleReactionTypes;
		lock (SyncRoot)
		{
			needsToRecalculatePossibleReactionTypes = invalidatePossibleReactionTypes;
		}
		if (needsToRecalculatePossibleReactionTypes)
		{
			RecalculatePossibleReactionTypes();
		}

		reactionFactors = null;
	}

	private void RecalculatePossibleReactionTypes()
	{
		HashSet<ChemicalType> chemicalTypes = resources.Keys.Select((rT) => rT.ChemicalType).ToHashSet();

		possibleReactionTypes = ReactionType.GetAllPossibleReactionTypes(chemicalTypes);

		lock (SyncRoot)
			invalidatePossibleReactionTypes = false;
	}

	private void InvalidatePossibleReactionTypes()
	{
		lock (SyncRoot)
		{
			invalidatePossibleReactionTypes = true;
		}
	}

	public void AddResources(IEnumerable<ResourceUnitData> resourceUnitDatas)
	{
		foreach (var resourceUnit in resourceUnitDatas)
		{
			AddResource(resourceUnit);
		}
	}

	public void AddResources(params ResourceUnitData[] resourceUnitDatas)
	{
		foreach (var resourceUnit in resourceUnitDatas)
		{
			AddResource(resourceUnit);
		}
	}

	public void AddResource(ResourceUnitData addedResourceUnitData)
	{
		var resourceUnit = resources.GetOrAdd(addedResourceUnitData.ResourceType, (_) =>
		{
			// Keep in mind: Whatever is in here may be called several times. Consider using Lazy to only perform actions once. https://medium.com/gft-engineering/correctly-using-concurrentdictionarys-addorupdate-method-94b7b41719d6
			ResourceUnit resourceUnit = new(addedResourceUnitData.ResourceType, this, new(addedResourceUnitData.ResourceType, 0, 0));

			resourceUnit.ResourceUnitChanged += ResourceUnit_Changed;

			InvalidateRecalcuables();

			ResourceCountChanged();

			return resourceUnit;
		});

		resourceUnit.Add(addedResourceUnitData);
	}

	private void ResourceCountChanged()
	{
		InvalidatePossibleReactionTypes();
	}

	public VolumeUnit VolumeOf(ResourceType resourceType)
	{
		if (resources.TryGetValue(resourceType, out var resourceUnit))
		{
			return resourceUnit.Volume;
		}
		else
		{
			return (VolumeUnit)DN.Zero;
		}
	}

	public MassUnit MassOf(ResourceType resourceType)
	{
		if (resources.TryGetValue(resourceType, out var resourceUnit))
		{
			return resourceUnit.Mass;
		}
		else
		{
			return (MassUnit)DN.Zero;
		}
	}

	public MolesUnit MolesOf(ResourceType resourceType)
	{
		if (resources.TryGetValue(resourceType, out var resourceUnit))
		{
			return resourceUnit.Moles;
		}
		else
		{
			return (MolesUnit)DN.Zero;
		}
	}

	private void ResourceUnit_Changed(ResourceUnit changed)
	{
		lock (SyncRoot)
		{
			InvalidateRecalcuables();

			if (resources[changed.ResourceType].Moles == (MolesUnit)DN.Zero)
			{
				ResourceCountChanged();
				changed.ResourceUnitChanged -= ResourceUnit_Changed;
				resources.RemoveStrict(changed.ResourceType);
			}
		}
	}

	private void InvalidateRecalcuables()
	{
		lock (SyncRoot)
		{
			recalculateVolume = true;
			recalculateMass = true;
			recalculatePressure = true;
			recalculateAverageTemperature = true;
			recalculateGasSubstanceAmount = true;
		}
	}

	public void TransferResourceByVolume(ResourceContainer targetContainer, VolumeUnit volumeToTransfer)
	{
		if (Volume - volumeToTransfer < 0)
		{
			throw new InvalidOperationException("Cannot transfer more resource volume than there is volume available.");
		}
		else if (volumeToTransfer < 0)
		{
			throw new ArgumentException("The volume to transfer must be larger than or equal to zero.", nameof(volumeToTransfer));
		}

		if (volumeToTransfer == 0)
		{
			return;
		}

		VolumeUnit intialVolume = Volume;

		DN desiredPartOfVolume = (DN)volumeToTransfer / (DN)intialVolume;

		foreach (var resourceUnit in resources.Values)
		{
			var moles = ChemicalType.MassToMoles(
						resourceUnit.ChemicalType,
						(MassUnit)(
							(DN)resourceUnit.Volume / (DN)intialVolume
								* desiredPartOfVolume
									* (DN)resourceUnit.ResourceType.Density)
								); // portion of current resource to transfer

			ResourceUnitData takenUnitData = new(resourceUnit.ResourceType, moles, (EnergyUnit)((DN)resourceUnit.InternalEnergy * desiredPartOfVolume));

			targetContainer.AddResource(takenUnitData);

			resourceUnit.Remove(takenUnitData);
		}

		Debug.Assert(DN.Difference((DN)intialVolume - (DN)Volume, (DN)volumeToTransfer) < (DN)acceptableVolumeTransferError, "Volume error too large!");
	}

	public void TransferResourceByVolume(ResourceContainer targetContainer, ResourceType resourceType, VolumeUnit volumeToTransfer)
	{
		if (Volume - volumeToTransfer < 0)
		{
			throw new InvalidOperationException("Cannot transfer more resource volume than there is volume available.");
		}
		else if (volumeToTransfer < 0)
		{
			throw new ArgumentException("The volume to transfer must be larger than or equal to zero.", nameof(volumeToTransfer));
		}

		if (volumeToTransfer == 0)
		{
			return;
		}

#if DEBUG
		VolumeUnit intialVolume = Volume;
#endif

		var unit = resources[resourceType];

		var portion = (DN)unit.Volume / (DN)volumeToTransfer;

		var moles = ChemicalType.MassToMoles(unit.ChemicalType, volumeToTransfer * resourceType.Density);

		ResourceUnitData takenUnitData = new(resourceType, moles, (EnergyUnit)((DN)unit.InternalEnergy * portion));

		targetContainer.AddResource(takenUnitData);

		unit.Remove(takenUnitData);

#if DEBUG
		Debug.Assert(DN.Difference((DN)intialVolume - (DN)Volume, (DN)volumeToTransfer) < (DN)acceptableVolumeTransferError, "Volume error too large!");
#endif
	}

	public void TransferAllResource(ResourceContainer targetContainer, ResourceType resourceType)
	{
		targetContainer.AddResource(TakeResourceByMoles(resourceType, resources[resourceType].Moles));
	}

	public void TransferAllResource(ResourceContainer targetContainer)
	{
		foreach (var resourceType in resources.Keys)
		{
			TransferAllResource(targetContainer, resourceType);
		}
	}

	public ResourceUnitData GetResourceUnitData(ResourceType resourceType)
	{
		if (resources.TryGetValue(resourceType, out var resourceUnit))
		{
			return resourceUnit.ResourceUnitData;
		}

		return new ResourceUnitData(resourceType, 0, 0);
	}

	public void Serialize(XmlWriter writer)
	{
		lock (SyncRoot)
		{
			writer.WriteStartElement(nameof(ResourceContainer));
			{
				writer.Serialize(MaxVolume, nameof(MaxVolume));

				writer.Serialize(resources.Values, (w, ru) => ru.ResourceUnitData.Serialize(w), nameof(resources));
			}
			writer.WriteEndElement();
		}
	}

	public static ResourceContainer Deserialize(XmlReader reader)
	{
		ResourceContainer resourceContainer = new();

		reader.ReadStartElement(nameof(ResourceContainer));
		{
			resourceContainer.maxVolume = reader.DeserializeUnit<VolumeUnit>(nameof(MaxVolume));

			reader.DeserializeCollection((r) => resourceContainer.AddResource(ResourceUnitData.Deserialize(r)), nameof(resources));
		}
		reader.ReadEndElement();

		return resourceContainer;
	}

	public void DoUIInspectorReadonly()
	{
		UIFunctions.BeginSub();
		{
			if (Mass == 0)
			{
				UIFunctions.PushDisabled();

				ImGui.Text("Empty");

				UIFunctions.PopEnabledOrDisabledState();
			}
			else
			{
				ImGui.Text($"{nameof(Mass)}: {Mass.FormatMass()}");
				ImGui.Text($"{nameof(Volume)}: {Volume.FormatVolume()}");
				ImGui.Text($"{nameof(MaxVolume)}: {MaxVolume.FormatVolume()}");
				ImGui.Text($"{nameof(Fullness)}: {Fullness.FormatPercentage()}");
				ImGui.Text($"{nameof(AverageTemperature)}: {AverageTemperature.FormatTemperature()}");
				ImGui.Text($"{nameof(Pressure)}: {Pressure.FormatPressure()}");
				ImGui.Text($"Different types of resources: {resources.Count}");
				ImGui.Separator();

				foreach (var resourceUnit in resources.Values)
				{
					resourceUnit.DoUIInspectorReadonly();
					ImGui.Separator();
				}
			}
		}
		UIFunctions.EndSub();
	}

	public IUIInspectable DoUIInspectorEditable()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<ResourceUnitData> EnumerateResources()
	{
		lock (SyncRoot)
		{
			foreach (var resourceUnit in resources.Values)
			{
				yield return resourceUnit.ResourceUnitData;
			}
		}
	}
}
