using ImGuiNET;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class ResourceContainer : IUIInspectable // Thread safe? Seems like changes to resources can occur while reactions are taking place... Is that okay?
{
	private VolumeUnit nonCompressableVolume; // TODO: check out the uses of this and change some of them to TotalVolume!
	/// <summary>
	/// [m³] sVolume occupied by liquids and solids (uncompressable matter) in cubic meters
	/// </summary>
	public VolumeUnit NonCompressableVolume
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateVolume)
				{
					RecalculateVolume(out var totalVolume, out var nonCompressableVolume, out var compressableOccupiedVolume);
					this.totalVolume = totalVolume;
					this.nonCompressableVolume = nonCompressableVolume;
					this.compressableOccupiedVolume = compressableOccupiedVolume;
				}

				return nonCompressableVolume;
			}
		}
	}

	private VolumeUnit totalVolume;
	public VolumeUnit TotalVolume
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateVolume)
				{
					RecalculateVolume(out var totalVolume, out var nonCompressableVolume, out var compressableOccupiedVolume);
					this.totalVolume = totalVolume;
					this.nonCompressableVolume = nonCompressableVolume;
					this.compressableOccupiedVolume = compressableOccupiedVolume;
				}

				return totalVolume;
			}
		}
	}

	private VolumeUnit compressableOccupiedVolume;
	public VolumeUnit CompressableOccupiedVolume
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateVolume)
				{
					RecalculateVolume(out var totalVolume, out var nonCompressableVolume, out var compressableOccupiedVolume);
					this.totalVolume = totalVolume;
					this.nonCompressableVolume = nonCompressableVolume;
					this.compressableOccupiedVolume = compressableOccupiedVolume;
				}

				return totalVolume;
			}
		}
	}

	private void RecalculateVolume(out VolumeUnit totalVolume, out VolumeUnit nonCompressableVolume, out VolumeUnit compressableOccupiedVolume)
	{
		lock (SyncRoot)
		{
			recalculateVolume = false;

			VolumeUnit compressableUncompressedVolume = 0; // The theoretical volume occupied by all compressables if they were not compressed.

			nonCompressableVolume = 0;

			foreach (var resourceUnit in resources.Values)
			{
				if (!resourceUnit.ResourceType.Compressable)
				{
					nonCompressableVolume += resourceUnit.NonCompressableVolume;
				}

				compressableUncompressedVolume += resourceUnit.UncompressedVolume;
			}

			if (compressableUncompressedVolume + nonCompressableVolume > VolumeCapacity)
			{  // The compressables uncompressed together with the non compressables use more than the available volume, forcing compressables to compress.
				totalVolume = VolumeCapacity; // Compressables compress to fill available space.
			}
			else
			{ // The compressables do not compress if there is plenty of space.
				totalVolume = nonCompressableVolume + compressableUncompressedVolume;
			}

			compressableOccupiedVolume = VolumeCapacity - nonCompressableVolume;
		}
	}

	private MassUnit mass;
	/// <summary>
	/// [kg] Mass in kilograms
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

	private VolumeUnit volumeCapacity;
	public VolumeUnit VolumeCapacity
	{
		get
		{
			lock (SyncRoot)
			{
				return volumeCapacity;
			}
		}
		set
		{
			Debug.Assert(value > 0, $"{nameof(VolumeCapacity)} cannot be negative.");
			lock (SyncRoot)
			{
				volumeCapacity = value;

				InvalidateRecalculables();
			}
		}
	}

	/// <summary>
	/// [m³] Volume which is not occupied by non compressable resources in cubic meters.
	/// </summary>
	public VolumeUnit NonCompressableUnoccupiedVolume => VolumeCapacity - NonCompressableVolume;

	/// <summary>
	/// [N/m²] Pressure within the container caused by gasses and liquids.
	/// </summary>
	public PressureUnit Pressure
	{
		get
		{
			lock (SyncRoot)
			{
				return Calculations.PressureIdealGasLaw(GasMoles, AverageGasTemperature, NonCompressableUnoccupiedVolume);
			}
		}
	}

	private TemperatureUnit averageTemperature;
	/// <summary>
	/// [K] The average temperature of all resources in this container in kelvin.
	/// </summary>
	public TemperatureUnit AverageTemperature
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateAverageTemperatures)
				{
					RecalculateAverageTemperatures(out var averageTemperature, out var averageGasTemperature);
					this.averageTemperature = averageTemperature;
					this.averageGasTemperature = averageGasTemperature;
				}

				return averageTemperature;
			}
		}
	}

	private TemperatureUnit averageGasTemperature;
	/// <summary>
	/// [K] The average temperature of gasses in this container in kelvin.
	/// </summary>
	public TemperatureUnit AverageGasTemperature
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateAverageTemperatures)
				{
					RecalculateAverageTemperatures(out var averageTemperature, out var averageGasTemperature);
					this.averageTemperature = averageTemperature;
					this.averageGasTemperature = averageGasTemperature;
				}

				return averageTemperature;
			}
		}
	}

	private void RecalculateAverageTemperatures(out TemperatureUnit averageTemperature, out TemperatureUnit averageGasTemperature)
	{
		lock (SyncRoot)
		{
			recalculateAverageTemperatures = false;

			averageTemperature = 0;
			averageGasTemperature = 0;

			foreach (var resourceUnit in resources.Values)
			{
				averageTemperature += (TemperatureUnit)((DN)resourceUnit.Temperature * (DN)(resourceUnit.Mass / Mass));

				if (resourceUnit.ResourceType.ChemicalPhase == ChemicalPhase.Gas)
				{
					averageGasTemperature += (TemperatureUnit)((DN)resourceUnit.Temperature * (DN)(resourceUnit.Mass / GasMass));
				}
			}
		}
	}

	private MolesUnit gasMoles;
	/// <summary>
	/// [mol] The substance amount of all gas resources in this container in moles.
	/// </summary>
	public MolesUnit GasMoles
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateGasMoles)
				{
					RecalculateGasSubstanceAmountAndMass(out MolesUnit gasSubstanceAmount, out MassUnit gasMass);
					this.gasMoles = gasSubstanceAmount;
					this.gasMass = gasMass;
				}

				return gasMoles;
			}
		}
	}

	private MassUnit gasMass;
	/// <summary>
	/// [kg] The total mass of gas in this container in kilograms.
	/// </summary>
	public MassUnit GasMass
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateGasMoles)
				{
					RecalculateGasSubstanceAmountAndMass(out MolesUnit gasSubstanceAmount, out MassUnit gasMass);
					this.gasMoles = gasSubstanceAmount;
					this.gasMass = gasMass;
				}

				return gasMass;
			}
		}
	}

	private void RecalculateGasSubstanceAmountAndMass(out MolesUnit gasSubstanceAmount, out MassUnit gasMass)
	{
		lock (SyncRoot)
		{
			recalculateGasMoles = false;

			gasSubstanceAmount = 0;
			gasMass = 0;

			foreach (var resourceUnit in resources.Values)
			{
				if (resourceUnit.ResourceType.ChemicalPhase == ChemicalPhase.Gas)
				{
					gasSubstanceAmount += resourceUnit.Moles;
					gasMass += resourceUnit.Mass;
				}
			}
		}
	}

	bool recalculateVolume = false;

	bool recalculateMass = false;

	bool recalculateIdealGasLaw = false;

	bool recalculateAverageTemperatures = false;

	bool recalculateGasMoles = false;

	public object SyncRoot = new();

	/// <summary>
	/// Fullness as a portion of the volume, considers only non compressable resources because compressable resources are infinetely compressable.
	/// </summary>
	public Portion<VolumeUnit> Fullness
	{
		get
		{
			lock (SyncRoot)
			{
				if ((DN)VolumeCapacity == 0)
				{
					return 1;
				}

				return NonCompressableVolume / VolumeCapacity;
			}
		}
	}

	private ConcurrentDictionary<ResourceType, ResourceUnit> resources = new();

	private static readonly VolumeUnit acceptableVolumeTransferError = 0.1;

	private static readonly DN permittedMaxPostReactionMassDiscrepancy = 0.001;

	public ResourceContainer(VolumeUnit maxVolume) : this()
	{
		this.volumeCapacity = maxVolume;
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

	private ILookup<Type, ReactionFactor>? reactionFactors;
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
		{
			invalidatePossibleReactionTypes = false;
		}
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
		lock (SyncRoot)
		{
			foreach (var resourceUnit in resourceUnitDatas)
			{
				AddResource(resourceUnit);
			}
		}
	}

	public void AddResources(params ResourceUnitData[] resourceUnitDatas)
	{
		lock (SyncRoot)
		{
			foreach (var resourceUnit in resourceUnitDatas)
			{
				AddResource(resourceUnit);
			}
		}
	}

	public void AddResource(ResourceUnitData addedResourceUnitData)
	{
		var resourceUnit = resources.GetOrAdd(addedResourceUnitData.ResourceType, (_) =>
		{
			// TODO: Keep in mind: Whatever is in here may be called several times. Consider using Lazy to only perform actions once. https://medium.com/gft-engineering/correctly-using-concurrentdictionarys-addorupdate-method-94b7b41719d6
			ResourceUnit resourceUnit = new(addedResourceUnitData.ResourceType, new(addedResourceUnitData.ResourceType, 0, 0));

			resourceUnit.ResourceUnitChanged += ResourceUnit_Changed;

			ResourceCountChanged();

			return resourceUnit;
		});

		lock (SyncRoot)
		{
			resourceUnit.Add(addedResourceUnitData);

			InvalidateRecalculables();
		}
	}

	private void ResourceCountChanged()
	{
		InvalidatePossibleReactionTypes();
	}

	public VolumeUnit VolumeOf(ResourceType resourceType)
	{
		if (resources.TryGetValue(resourceType, out var resourceUnit))
		{
			return resourceUnit.NonCompressableVolume;
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
			InvalidateRecalculables();

			if (resources[changed.ResourceType].Moles == (MolesUnit)DN.Zero)
			{
				ResourceCountChanged();
				changed.ResourceUnitChanged -= ResourceUnit_Changed;
				resources.RemoveStrict(changed.ResourceType);
			}
		}
	}

	public void InvalidateRecalculables()
	{
		lock (SyncRoot)
		{
			recalculateVolume = true;
			recalculateMass = true;
			recalculateIdealGasLaw = true;
			recalculateAverageTemperatures = true;
			recalculateGasMoles = true;
		}
	}

	public void TransferResourceByVolume(ResourceContainer targetContainer, VolumeUnit volumeToTransfer)
	{
		if (NonCompressableVolume - volumeToTransfer < 0)
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

		VolumeUnit intialVolume = NonCompressableVolume;

		DN desiredPartOfVolume = (DN)volumeToTransfer / (DN)intialVolume;

		foreach (var resourceUnit in resources.Values)
		{
			var moles = ChemicalType.MassToMoles(
						resourceUnit.ChemicalType,
						(MassUnit)(
							(DN)resourceUnit.NonCompressableVolume / (DN)intialVolume
								* desiredPartOfVolume
									* (DN)resourceUnit.ResourceType.Density)
								); // portion of current resource to transfer

			ResourceUnitData takenUnitData = new(resourceUnit.ResourceType, moles, (EnergyUnit)((DN)resourceUnit.InternalEnergy * desiredPartOfVolume));

			targetContainer.AddResource(takenUnitData);

			resourceUnit.Remove(takenUnitData);
		}

		Debug.Assert(DN.Difference((DN)intialVolume - (DN)NonCompressableVolume, (DN)volumeToTransfer) < (DN)acceptableVolumeTransferError, "Volume error too large!");
	}

	public void TransferResourceByVolume(ResourceContainer targetContainer, ResourceType resourceType, VolumeUnit volumeToTransfer)
	{
		if (NonCompressableVolume - volumeToTransfer < 0)
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
		VolumeUnit intialVolume = NonCompressableVolume;
#endif

		var unit = resources[resourceType];

		var portion = (DN)unit.NonCompressableVolume / (DN)volumeToTransfer;

		var moles = ChemicalType.MassToMoles(unit.ChemicalType, volumeToTransfer * resourceType.Density);

		ResourceUnitData takenUnitData = new(resourceType, moles, (EnergyUnit)((DN)unit.InternalEnergy * portion));

		targetContainer.AddResource(takenUnitData);

		unit.Remove(takenUnitData);

#if DEBUG
		Debug.Assert(DN.Difference((DN)intialVolume - (DN)NonCompressableVolume, (DN)volumeToTransfer) < (DN)acceptableVolumeTransferError, "Volume error too large!");
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
				writer.Serialize(VolumeCapacity, nameof(VolumeCapacity));

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
			resourceContainer.volumeCapacity = reader.DeserializeUnit<VolumeUnit>(nameof(VolumeCapacity));

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

				ImGui.Text("Container is empty");

				ImGui.Separator();
			}

			ImGui.Text($"{nameof(Mass)}: {Mass.FormatMass()}");
			ImGui.Text($"{nameof(GasMass)}: {GasMass.FormatMass()}");
			ImGui.Text($"{nameof(VolumeCapacity)}: {VolumeCapacity.FormatVolume()}");
			ImGui.Text($"{nameof(TotalVolume)}: {TotalVolume.FormatVolume()}");
			ImGui.Text($"{nameof(Fullness)}: {Fullness.FormatPercentage()}");
			ImGui.Text($"{nameof(NonCompressableVolume)}: {NonCompressableVolume.FormatVolume()}");
			ImGui.Text($"{nameof(CompressableOccupiedVolume)}: {CompressableOccupiedVolume.FormatVolume()}");
			ImGui.Text($"{nameof(NonCompressableUnoccupiedVolume)}: {NonCompressableUnoccupiedVolume.FormatVolume()}");
			ImGui.Text($"{nameof(AverageTemperature)}: {AverageTemperature.FormatTemperature()}");
			ImGui.Text($"{nameof(AverageGasTemperature)}: {AverageGasTemperature.FormatTemperature()}");
			ImGui.Text($"{nameof(Pressure)}: {Pressure.FormatPressure()}");
			ImGui.Text($"Different types of resources: {resources.Count}");
			ImGui.Separator();

			foreach (var resourceUnit in resources.Values)
			{
				resourceUnit.DoUIInspectorReadonly();
				ImGui.Separator();
			}

			if (Mass == 0)
			{
				UIFunctions.PopEnabledOrDisabledState();
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

	/// <summary>
	/// Adds and distributes a total amount of energy evenly among all resources.
	/// </summary>
	/// <param name="energyUnit">[J]</param>
	public void AddEnergy(EnergyUnit energyUnit)
	{
		lock (SyncRoot)
		{
			foreach (var resource in resources.Values)
			{
				AddResource(new(resource.ResourceType, 0, energyUnit * (Portion<EnergyUnit>)(DN)(resource.Mass / Mass)));
			}
		}
	}
}
