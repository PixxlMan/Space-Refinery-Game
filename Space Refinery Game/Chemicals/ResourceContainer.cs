using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class ResourceContainer : IUIInspectable
	{
		private DecimalNumber volume;
		public DecimalNumber Volume
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

		private DecimalNumber RecalculateVolume()
		{
			recalculateVolume = false;
			
			DecimalNumber volume = 0;

			foreach (var resourceUnit in resources.Values)
			{
				volume += resourceUnit.Volume;
			}

			return volume;
		}

		private DecimalNumber mass;
		public DecimalNumber Mass
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

		private DecimalNumber RecalculateMass()
		{
			DecimalNumber mass = 0;

			foreach (var resourceUnit in resources.Values)
			{
				mass += resourceUnit.Mass;
			}

			return mass;
		}

		private DecimalNumber maxVolume;
		public DecimalNumber MaxVolume => maxVolume;

		public DecimalNumber FreeVolume => DecimalNumber.Max(MaxVolume - Volume, 0);

		bool recalculateVolume = false;

		bool recalculateMass = false;

		public object SyncRoot = new();

		//public event Action CompositionChanged

		public DecimalNumber Fullness
		{
			get
			{
				lock (SyncRoot)
				{
					if (MaxVolume == 0)
					{
						return 1;
					}

					return Volume / MaxVolume;
				}
			}
		}

		private ConcurrentDictionary<ResourceType, ResourceUnit> resources = new();

		private static readonly DecimalNumber acceptableVolumeTransferError = 0.1;

		public ResourceContainer(DecimalNumber maxVolume)
		{
			this.maxVolume = maxVolume;
		}

		private ResourceContainer()
		{

		}

		public ResourceUnitData TakeResourceByMoles(ResourceType resourceType, DecimalNumber moles)
		{
			if (moles == DecimalNumber.Zero)
			{
				return new ResourceUnitData(resourceType, DecimalNumber.Zero);
			}

			resources[resourceType].Remove(new(resourceType, moles));

			return new(resourceType, moles);
		}

		public void AddResources(IEnumerable<ResourceUnitData> resourceUnitDatas)
		{
			foreach (var resourceUnit in resourceUnitDatas)
			{
				AddResource(resourceUnit);
			}
		}

		public void AddResource(ResourceUnitData resourceUnitData)
		{
			lock (SyncRoot)
			{
				if (Volume + resourceUnitData.Volume > MaxVolume + acceptableVolumeTransferError)
				{
					throw new InvalidOperationException("Cannot exceed maximum volume.");
				}
			}

			resources.AddOrUpdate(resourceUnitData.ResourceType, (_) =>
			{
				ResourceUnit resourceUnit = new(resourceUnitData.ResourceType, this, resourceUnitData);

				resourceUnit.ResourceUnitChanged += ResourceUnit_Changed;

				InvalidateRecalcuables();

				return resourceUnit;
			},
			(_, ru) =>
			{
				lock (SyncRoot)
					ru.Add(resourceUnitData);

				return ru;
			});
		}

		public DecimalNumber VolumeOf(ResourceType resourceType)
		{
			if (resources.TryGetValue(resourceType, out var resourceUnit))
			{
				return resourceUnit.Volume;
			}
			else
			{
				return DecimalNumber.Zero;
			}
		}

		private void ResourceUnit_Changed(ResourceUnit changed)
		{
			lock (SyncRoot)
			{
				InvalidateRecalcuables();

				if (resources[changed.ResourceType].Moles == DecimalNumber.Zero)
				{
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
			}
		}

		public void TransferResourceByVolume(ResourceContainer targetContainer, DecimalNumber volume)
		{
			if (Volume - volume < 0)
			{
				throw new InvalidOperationException("Cannot transfer more resource volume than there is volume available.");
			}
			else if (volume < 0)
			{
				throw new ArgumentException("The volume to transfer must be larger than or equal to zero.", nameof(volume));
			}

			if (volume == DecimalNumber.Zero)
			{
				return;
			}

			DecimalNumber intialVolume = Volume;

			DecimalNumber desiredPartOfVolume = (volume / intialVolume);

			foreach (var resourceUnit in resources.Values)
			{
				var moles = ChemicalType.MassToMoles(
							resourceUnit.ChemicalType,
							(resourceUnit.Volume / intialVolume)
								* desiredPartOfVolume
									* resourceUnit.ResourceType.Density); // portion of current resource to transfer

				targetContainer.AddResource(new(resourceUnit.ResourceType, moles));

				resources[resourceUnit.ResourceType].Remove(new(resourceUnit.ResourceType, moles));
			}

			Debug.Assert(DecimalNumber.Difference(intialVolume - Volume, volume) < acceptableVolumeTransferError, "Volume error too large!");
		}

		public void TransferResourceByVolume(ResourceContainer targetContainer, ResourceType resourceType, DecimalNumber volume)
		{
			if (Volume - volume < 0)
			{
				throw new InvalidOperationException("Cannot transfer more resource volume than there is volume available.");
			}
			else if (volume < 0)
			{
				throw new ArgumentException("The volume to transfer must be larger than or equal to zero.", nameof(volume));
			}

			if (volume == DecimalNumber.Zero)
			{
				return;
			}

#if DEBUG
			DecimalNumber intialVolume = Volume;
#endif

			var resourceUnit = resources[resourceType];

			var moles = ChemicalType.MassToMoles(resourceUnit.ChemicalType, volume * resourceUnit.ResourceType.Density); // moles of resource to transfer

			targetContainer.AddResource(new(resourceUnit.ResourceType, moles));

			resources[resourceType].Remove(new(resourceType, moles));

			Debug.Assert(DecimalNumber.Difference(intialVolume - Volume, volume) < acceptableVolumeTransferError, "Volume error too large!");
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

			return new ResourceUnitData(resourceType, 0);
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
				resourceContainer.maxVolume = reader.DeserializeDecimalNumber(nameof(MaxVolume));

				reader.DeserializeCollection((r) => resourceContainer.AddResource(ResourceUnitData.Deserialize(r)), nameof(resources));
			}
			reader.ReadEndElement();

			return resourceContainer;
		}

		public void DoUIInspectorReadonly()
		{
			UIFunctions.BeginSub();
			{
				if (Mass == DecimalNumber.Zero)
				{
					UIFunctions.PushDisabled();

					ImGui.Text("Empty");

					UIFunctions.PopDisabled();
				}
				else
				{
					ImGui.Text($"{nameof(Mass)}: {Mass} kg");
					ImGui.Text($"{nameof(Volume)}: {Volume} m³");
					ImGui.Text($"{nameof(MaxVolume)}: {MaxVolume} m³");
					ImGui.Text($"{nameof(Fullness)}: {Fullness}");
					ImGui.Text($"Different types of resources: {resources.Count}");

					foreach (var resourceUnit in resources.Values)
					{
						resourceUnit.DoUIInspectorReadonly();
					}
				}
			}
			UIFunctions.EndSub();
		}

		public IUIInspectable DoUIInspectorEditable()
		{
			throw new NotImplementedException();
		}
	}
}
