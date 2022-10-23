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
		private DecimalNumber volume;
		public DecimalNumber Volume { get => volume; }

		private DecimalNumber mass;
		public DecimalNumber Mass { get => mass; }

		private DecimalNumber maxVolume;
		public DecimalNumber MaxVolume => maxVolume;

		public DecimalNumber Fullness
		{
			get
			{
				if (MaxVolume == 0)
				{
					return 1;
				}

				return Volume / MaxVolume;
			}
		}

		private Dictionary<ResourceType, ResourceUnit> resources = new();

		public ResourceContainer(DecimalNumber maxVolume)
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
				resources[unit.ResourceType].Add(unit);
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

		public ResourceUnit ExtractResourceByVolume(ResourceType resourceType, DecimalNumber extractionVolume)
		{
			DecimalNumber transferPart = extractionVolume / resources[resourceType].Volume;

			var extractedResource = ResourceUnit.GetPart(resources[resourceType], transferPart);

			resources[resourceType].Moles -= extractedResource.Moles;

			mass -= extractedResource.Mass;

			if (resources[resourceType].Mass == 0)
			{
				resources.Remove(resourceType);
			}

			volume -= extractionVolume;

			return extractedResource;
		}

		public ResourceUnit ExtractResourceByMoles(ResourceType resourceType, DecimalNumber extractionMoles)
		{
			resources[resourceType].Moles -= extractionMoles;

			mass -= ChemicalType.MolesToMass(resourceType.ChemicalType, extractionMoles);

			if (resources[resourceType].Mass <= 0)
			{
				resources.Remove(resourceType);
			}

			volume -= ChemicalType.MolesToMass(resourceType.ChemicalType, extractionMoles);

			var extracted = resources[resourceType].Clone();

			extracted.Moles = extractionMoles;

			return extracted;
		}

		public void TransferResource(ResourceContainer transferTarget, DecimalNumber transferVolume)
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
