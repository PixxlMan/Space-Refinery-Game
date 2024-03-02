using ImGuiNET;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class ResourceContainer : IUIInspectable // Thread safe? Seems like changes to resources can occur while reactions are taking place... Is that okay?
{
	private VolumeUnit nonCompressableVolume; // TODO: check out the uses of this and change some of them to TotalVolume!
	/// <summary>
	/// Volume occupied by liquids and solids (uncompressable matter) in cubic meters [m³]
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
				InvalidateRecalcuables();

				volumeCapacity = value;
			}
		}
	}

	/// <summary>
	/// Volume which is not occupied by non compressable resources in [m³]
	/// </summary>
	public VolumeUnit NonCompressableUnoccupiedVolume => VolumeCapacity - NonCompressableVolume;

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

	private PressureUnit RecalculatePressure()
	{
		lock (SyncRoot)
		{
			return Calculations.CalculatePressureUsingIdealGasLaw(GasSubstanceAmount, GasTemperature, NonCompressableUnoccupiedVolume);
		}
	}

	private TemperatureUnit averageTemperature;
	/// <summary>
	/// [K] The average temperature of all non gaseous resources in this container in kelvin
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

	private TemperatureUnit RecalculateAverageTemperature()
	{
		lock (SyncRoot)
		{
			recalculateAverageTemperature = false;

			TemperatureUnit averageNonGasTemperature = 0;

			foreach (var resourceUnit in resources.Values)
			{
				if (resourceUnit.ResourceType.ChemicalPhase != ChemicalPhase.Gas)
				{
					averageNonGasTemperature += (TemperatureUnit)((DN)resourceUnit.NonGasTemperature * (DN)(resourceUnit.Mass / Mass));
				}
			}

			averageNonGasTemperature += (TemperatureUnit)((DN)GasTemperature * (DN)(GasMass / Mass));

			return averageNonGasTemperature;
		}
	}

	private TemperatureUnit gasTemperature;
	/// <summary>
	/// [K] The temperature of all gaseous resources in this container in kelvin
	/// </summary>
	public TemperatureUnit GasTemperature
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateGasTemperature)
				{
					gasTemperature = RecalculateGasTemperature();
				}

				return gasTemperature;
			}
		}
	}

	private TemperatureUnit RecalculateGasTemperature()
	{
		lock (SyncRoot)
		{
			recalculateGasTemperature = false;

			if (GasMass == 0 || GasSubstanceAmount == 0)
			{
				return 0;
			}
			else
			{
				return Calculations.TemperatureIdealGasLaw(GasSubstanceAmount, Pressure, NonCompressableUnoccupiedVolume);
			}
		}
	}

	private MolesUnit gasSubstanceAmount;
	/// <summary>
	/// The substance amount of all gas resources in this container in mols [mol]
	/// </summary>
	public MolesUnit GasSubstanceAmount // TODO: rename to GasMoles for constistency with Moles
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateGasSubstanceAmountAndMass)
				{
					RecalculateGasSubstanceAmountAndMass(out MolesUnit gasSubstanceAmount, out MassUnit gasMass);
					this.gasSubstanceAmount = gasSubstanceAmount;
					this.gasMass = gasMass;
				}

				return gasSubstanceAmount;
			}
		}
	}

	private MassUnit gasMass;
	/// <summary>
	/// 
	/// </summary>
	public MassUnit GasMass
	{
		get
		{
			lock (SyncRoot)
			{
				if (recalculateGasSubstanceAmountAndMass)
				{
					RecalculateGasSubstanceAmountAndMass(out MolesUnit gasSubstanceAmount, out MassUnit gasMass);
					this.gasSubstanceAmount = gasSubstanceAmount;
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
			recalculateGasSubstanceAmountAndMass = false;

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

	bool recalculatePressure = false;

	bool recalculateAverageTemperature = false;

	bool recalculateGasTemperature = false;

	bool recalculateGasSubstanceAmountAndMass = false;

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
			InvalidateRecalcuables();

			if (resources[changed.ResourceType].Moles == (MolesUnit)DN.Zero)
			{
				ResourceCountChanged();
				changed.ResourceUnitChanged -= ResourceUnit_Changed;
				resources.RemoveStrict(changed.ResourceType);
			}
		}
	}

	public void InvalidateRecalcuables()
	{
		lock (SyncRoot)
		{
			recalculateVolume = true;
			recalculateMass = true;
			recalculatePressure = true;
			recalculateAverageTemperature = true;
			recalculateGasTemperature = true;
			recalculateGasSubstanceAmountAndMass = true;
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
			ImGui.Text($"{nameof(GasTemperature)}: {GasTemperature.FormatTemperature()}");
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
}
