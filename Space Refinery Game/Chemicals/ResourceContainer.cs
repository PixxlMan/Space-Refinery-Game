using FixedPrecision;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class ResourceContainer : IUIInspectable
	{
		private DecimalNumber volume;
		public DecimalNumber Volume { get => volume; }

		private DecimalNumber mass;
		public DecimalNumber Mass { get => mass; }

		private DecimalNumber maxVolume;
		public DecimalNumber MaxVolume => maxVolume;

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

		public ResourceContainer(DecimalNumber maxVolume)
		{
			this.maxVolume = maxVolume;
		}

		private ResourceContainer()
		{

		}

		public void AddResource(ResourceUnit unit)
		{
			lock (SyncRoot)
			{
				if (Volume + unit.Volume > MaxVolume)
				{
					throw new Exception("Operation would make volume larger than max volume.");
				}

				if (resources.ContainsKey(unit.ResourceType))
				{
					resources[unit.ResourceType].Add(unit);
				}
				else
				{
					resources.AddUnique(unit.ResourceType, unit);
				}

				if (resources[unit.ResourceType].Mass == 0)
				{
					resources.RemoveStrict(unit.ResourceType);
				}

				volume += unit.Volume;

				mass += unit.Mass;
			}
		}

		public bool FillResource(ResourceUnit unit, out ResourceUnit rest)
		{
			lock (SyncRoot)
			{
				bool usedEntireUnit;

				ResourceUnit fillable;

				if (Volume + unit.Volume > MaxVolume)
				{
					usedEntireUnit = false;

					fillable = ResourceUnit.GetPart(unit, (MaxVolume - Volume) / unit.Volume);

					rest = new(unit.ResourceType, unit.Moles - fillable.Moles, unit.InternalEnergy - fillable.Moles);
				}
				else
				{
					usedEntireUnit = true;

					fillable = unit;

					rest = new(unit.ResourceType, 0, 0);
				}

				AddResource(fillable);

				return usedEntireUnit;
			}
		}

		public ResourceUnit GetResourceUnitForResourceType(ResourceType resourceType)
		{
			if (!resources.ContainsKey(resourceType))
			{
				return new ResourceUnit(resourceType, 0, 0);
			}

			return resources[resourceType];
		}

		public bool ContainsResourceType(ResourceType resourceType)
		{
			return resources.ContainsKey(resourceType);
		}

		public ResourceUnit ExtractResourceByVolume(ResourceType resourceType, DecimalNumber extractionVolume)
		{
			lock (SyncRoot)
			{
				if (!resources.ContainsKey(resourceType))
				{
					return new ResourceUnit(resourceType, 0, 0);
				}

				DecimalNumber transferPart = extractionVolume / resources[resourceType].Volume;

				var extractedResource = ResourceUnit.GetPart(resources[resourceType], transferPart);

				resources[resourceType].Moles -= extractedResource.Moles;

				mass -= extractedResource.Mass;

				volume -= extractionVolume;

				if (resources[resourceType].Moles == 0)
				{
					resources.RemoveStrict(resourceType);
				}

				return extractedResource;
			}
		}

		public ResourceUnit ExtractResourceByMoles(ResourceType resourceType, DecimalNumber extractionMoles)
		{
			lock (SyncRoot)
			{
				if (!resources.ContainsKey(resourceType))
				{
					return new ResourceUnit(resourceType, 0, 0);
				}

				if (resources[resourceType].Moles - extractionMoles < 0)
				{
					throw new Exception("Cannot extract more moles than there are available.");
				}

				resources[resourceType].Moles -= extractionMoles;

				mass -= ChemicalType.MolesToMass(resourceType.ChemicalType, extractionMoles);

				volume -= ChemicalType.MolesToMass(resourceType.ChemicalType, extractionMoles);

				var extracted = resources[resourceType].Clone();

				extracted.Moles = extractionMoles;

				if (resources[resourceType].Moles <= 0)
				{
					resources.RemoveStrict(resourceType);
				}

				return extracted;
			}
		}

		public void TransferResource(ResourceContainer transferTarget, DecimalNumber transferVolume)
		{
			lock (SyncRoot)
			{
				if (transferTarget.Volume + transferVolume > transferTarget.MaxVolume)
				{
					throw new Exception("Operation would make volume larger than max volume.");
				}

				if (transferVolume > Volume)
				{
					throw new ArgumentException("Requested volume to transfer greater than total available volume.", nameof(transferVolume));
				}

				if (transferVolume == 0 || Volume == 0)
				{
					return;
				}

				DecimalNumber transferPart = (transferVolume / Volume);

				foreach (var unit in resources.Values)
				{
					var transferedResource = ResourceUnit.GetPart(unit, transferPart);

					transferTarget.AddResource(transferedResource);

					resources[unit.ResourceType].Remove(transferedResource);

					mass -= transferedResource.Mass;

					if (resources[unit.ResourceType].Mass == 0)
					{
						resources.RemoveStrict(unit.ResourceType);
					}
				}

				volume -= transferVolume;
			}
		}

		public override string ToString()
		{
			lock (SyncRoot)
			{
				string str = "\n";

				str += $"Mass: {Mass} kg\n";
				str += $"Volume: {Volume} m3\n";
				str += $"Max Volume: {MaxVolume} m3\n";
				str += $"Fullness: {Fullness}\n";

				str += "ResourceContainer contains: \n";

				foreach (var resourceMassPair in resources)
				{
					str += $"{resourceMassPair.Key.ResourceName}: {resourceMassPair.Value.Mass} kg ~ {resourceMassPair.Value.Volume} m3\n";
					str += $"Enthalpy: {resourceMassPair.Value.Enthalpy} J\n";
					str += $"Temperature: {(resourceMassPair.Value.Mass > 0 ? resourceMassPair.Value.Temperature : @"N\A")} k\n";
					str += $"Pressure: {resourceMassPair.Value.Pressure} Pa\n";
					str += $"Internal Energy: {resourceMassPair.Value.InternalEnergy} kJ\n";
				}

				return str;
			}
		}

		public void DoUIInspectorReadonly()
		{
			throw new NotImplementedException();
		}

		public IUIInspectable DoUIInspectorEditable()
		{
			throw new NotImplementedException();
		}

		public void Serialize(XmlWriter writer)
		{
			lock (SyncRoot)
			{
				writer.WriteStartElement(nameof(ResourceContainer));
				{
					writer.Serialize(MaxVolume, nameof(MaxVolume));

					writer.Serialize(resources.Values, (w, r) => r.Serialize(w), nameof(resources));
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

				reader.DeserializeCollection((r) => resourceContainer.AddResource(ResourceUnit.Deserialize(r)), nameof(resources));
			}
			reader.ReadEndElement();

			return resourceContainer;
		}
	}
}
