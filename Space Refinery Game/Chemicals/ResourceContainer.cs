using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class ResourceContainer : IUIInspectable
	{
		private FixedDecimalLong8 volume;
		public FixedDecimalLong8 Volume { get => volume; }

		private FixedDecimalLong8 mass;
		public FixedDecimalLong8 Mass { get => mass; }

		private FixedDecimalLong8 maxVolume;
		public FixedDecimalLong8 MaxVolume => maxVolume;

		public FixedDecimalLong8 Fullness
		{
			get
			{
				if (MaxVolume == 0)
				{
					return 1;
				}

				return Volume / (FixedDecimalLong8)MaxVolume;
			}
		}

		private Dictionary<ResourceType, ResourceUnit> resources = new();

		public ResourceContainer(FixedDecimalLong8 maxVolume)
		{
			this.maxVolume = maxVolume;
		}

		private ResourceContainer()
		{

		}

		public void AddResource(ResourceUnit unit)
		{
			if (Volume + unit.Volume > MaxVolume)
			{
				throw new Exception("Operation would make volume larger than max volume.");
			}

			if (resources.ContainsKey(unit.ResourceType))
			{
				resources[unit.ResourceType] += unit;
			}
			else
			{
				resources.Add(unit.ResourceType, unit);
			}

			if (resources[unit.ResourceType].Mass == 0)
			{
				resources.Remove(unit.ResourceType);
			}

			volume += unit.Volume;

			mass += unit.Mass;
		}

		public ResourceUnit GetResourceUnitForResourceType(ResourceType resourceType)
		{
			return resources[resourceType];
		}

		public bool ContainsResourceType(ResourceType resourceType)
		{
			return resources.ContainsKey(resourceType);
		}

		public ResourceUnit ExtractResource(ResourceType resourceType, FixedDecimalLong8 extractionVolume)
		{
			FixedDecimalLong8 transferPart = extractionVolume / resources[resourceType].Volume;

			var extractedResource = ResourceUnit.Part(resources[resourceType], transferPart);

			resources[resourceType] -= extractedResource;

			mass -= extractedResource.Mass;

			if (resources[resourceType].Mass == 0)
			{
				resources.Remove(resourceType);
			}

			volume -= extractionVolume;

			return extractedResource;
		}

		public void TransferResource(ResourceContainer transferTarget, FixedDecimalLong8 transferVolume)
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

			FixedDecimalLong8 transferPart = (transferVolume / Volume);

			foreach (var unit in resources.Values)
			{
				var transferedResource = ResourceUnit.Part(unit, transferPart);

				transferTarget.AddResource(transferedResource);

				resources[unit.ResourceType] -= transferedResource;

				mass -= transferedResource.Mass;

				if (resources[unit.ResourceType].Mass == 0)
				{
					resources.Remove(unit.ResourceType);
				}
			}

			volume -= transferVolume;
		}

		public override string ToString()
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
				str += $"Temperature: {resourceMassPair.Value.Temperature} k\n";
				str += $"Pressure: {resourceMassPair.Value.Pressure} Pa\n";
				str += $"Internal Energy: {resourceMassPair.Value.InternalEnergy} kJ\n";
			}

			return str;
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
			writer.WriteStartElement(nameof(ResourceContainer));
			{
				writer.Serialize(MaxVolume, nameof(MaxVolume));

				writer.Serialize(resources.Values, (w, r) => r.Serialize(w), nameof(resources));
			}
			writer.WriteEndElement();
		}

		public static ResourceContainer Deserialize(XmlReader reader, MainGame mainGame)
		{
			ResourceContainer resourceContainer = new();

			reader.ReadStartElement(nameof(ResourceContainer));
			{
				resourceContainer.maxVolume = reader.DeserializeFixedDecimalLong8(nameof(MaxVolume));

				reader.DeserializeCollection((r) => resourceContainer.AddResource(ResourceUnit.Deserialize(r, mainGame)), nameof(resources));
			}
			reader.ReadEndElement();

			return resourceContainer;
		}
	}
}
