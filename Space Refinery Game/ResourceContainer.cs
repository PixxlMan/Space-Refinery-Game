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
		private FixedDecimalLong8 volume;
		public FixedDecimalLong8 Volume { get => volume; }

		private FixedDecimalInt4 mass;
		public FixedDecimalInt4 Mass { get => mass; }

		public FixedDecimalInt4 MaxVolume;

		private Dictionary<ResourceType, FixedDecimalInt4> resources = new();

		public ResourceContainer(FixedDecimalInt4 maxVolume)
		{
			MaxVolume = maxVolume;
		}

		public void AddResource(ResourceType resourceType, FixedDecimalInt4 mass)
		{
			if (Volume + ((FixedDecimalLong8)mass / resourceType.Density) > (FixedDecimalLong8)MaxVolume)
			{
				throw new Exception("Operation would make volume larger than max volume-");
			}

			if (resources.ContainsKey(resourceType))
			{
				resources[resourceType] += mass;
			}
			else
			{
				resources.Add(resourceType, mass);
			}

			volume += (FixedDecimalLong8)mass / resourceType.Density;

			this.mass += mass;
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

			var transferPart = transferVolume / Volume;

			foreach (var resourceMassPair in resources)
			{
				var massTransfer = (FixedDecimalInt4)((FixedDecimalLong8)resourceMassPair.Value * transferPart);

				transferTarget.AddResource(resourceMassPair.Key, massTransfer);

				resources[resourceMassPair.Key] -= massTransfer;

				mass -= massTransfer;
			}

			volume -= transferVolume;
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
