using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class ResourceContainer : IUIInspectable
	{
		private FixedDecimalLong8 volume;
		public FixedDecimalLong8 Volume { get => volume; }

		private FixedDecimalInt4 mass;
		public FixedDecimalInt4 Mass { get => mass; }

		public readonly FixedDecimalInt4 MaxVolume;

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

		public ResourceContainer(FixedDecimalInt4 maxVolume)
		{
			MaxVolume = maxVolume;
		}

		public void AddResource(ResourceUnit unit)
		{
			if (Volume + unit.Volume > (FixedDecimalLong8)MaxVolume)
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
			FixedDecimalInt4 transferPart = (FixedDecimalInt4)(extractionVolume / resources[resourceType].Volume);

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
			if (transferTarget.Volume + transferVolume > (FixedDecimalLong8)transferTarget.MaxVolume)
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
	}
}
