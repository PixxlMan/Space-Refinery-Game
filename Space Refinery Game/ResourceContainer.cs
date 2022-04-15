using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class ResourceContainer
	{
		private bool hasVolumeChangedSinceCache = true;

		FixedDecimalInt4 cachedVolume;
		public FixedDecimalInt4 GetVolume()
		{
			if (!hasVolumeChangedSinceCache)
			{
				return cachedVolume;
			}

			hasVolumeChangedSinceCache = false;

			FixedDecimalLong8 volume = 0;

			foreach (var resourceMassPair in resources)
			{
				volume += (FixedDecimalLong8)resourceMassPair.Value * resourceMassPair.Key.Density;
			}

			cachedVolume = (FixedDecimalInt4)volume;

			return (FixedDecimalInt4)volume;
		}

		private FixedDecimalInt4 mass;
		public FixedDecimalInt4 Mass { get => mass; }

		private Dictionary<ResourceType, FixedDecimalInt4> resources = new();

		public void AddResource(ResourceType resourceType, FixedDecimalInt4 mass)
		{
			if (resources.ContainsKey(resourceType))
			{
				resources[resourceType] += mass;
			}
			else
			{
				resources.Add(resourceType, mass);
			}

			this.mass += mass;

			hasVolumeChangedSinceCache = true;
		}

		public void TransferResource(ResourceContainer transferTarget, FixedDecimalInt4 transferVolume)
		{
			var transferPart = transferVolume / GetVolume();

			foreach (var resourceMassPair in resources)
			{
				var massTransfer = resourceMassPair.Value * transferPart;

				transferTarget.AddResource(resourceMassPair.Key, massTransfer);

				resources[resourceMassPair.Key] -= massTransfer;
			}

			hasVolumeChangedSinceCache = true;
		}
	}
}
