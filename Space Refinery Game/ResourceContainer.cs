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

		FixedDecimalLong8 cachedVolume;
		public FixedDecimalLong8 GetVolume()
		{
			if (!hasVolumeChangedSinceCache)
			{
				return cachedVolume;
			}

			FixedDecimalLong8 volume = 0;

			foreach (var resourceMassPair in resources)
			{
				volume += (FixedDecimalLong8)resourceMassPair.Value / resourceMassPair.Key.Density;
			}

			cachedVolume = volume;

			hasVolumeChangedSinceCache = false;

			return volume;
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

		public void TransferResource(ResourceContainer transferTarget, FixedDecimalLong8 transferVolume)
		{
			if (transferVolume > GetVolume())
			{
				throw new ArgumentException("Requested volume to transfer larger than total available volume.", nameof(transferVolume));
			}

			if (transferVolume == 0 || GetVolume() == 0)
			{
				return;
			}

			var transferPart = transferVolume / GetVolume();

			foreach (var resourceMassPair in resources)
			{
				var massTransfer = (FixedDecimalInt4)((FixedDecimalLong8)resourceMassPair.Value * transferPart);

				transferTarget.AddResource(resourceMassPair.Key, massTransfer);

				resources[resourceMassPair.Key] -= massTransfer;
			}

			hasVolumeChangedSinceCache = true;
		}

		public override string ToString()
		{
			string str = string.Empty;

			str += "ResourceContainer contains: \n";

			foreach (var resourceMassPair in resources)
			{
				str += $"{resourceMassPair.Key.ResourceName}: {resourceMassPair.Value} kg";
				str += "\n";
			}

			return str;
		}
	}
}
