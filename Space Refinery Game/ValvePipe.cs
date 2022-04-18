using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	public sealed class ValvePipe : Pipe
	{
		private ValvePipe()
		{
			informationProvider = new ValvePipeInformationProvider(this);
		}

		public FixedDecimalLong8 Limiter = (FixedDecimalLong8)0.5;

		public override void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, Connector sourceConnector)
		{
			source.TransferResource(ResourceContainers[sourceConnector], volume);
		}

		public Dictionary<Connector, ResourceContainer> ResourceContainers = new();

		protected override void SetUp()
		{
			foreach (var connector in Connectors)
			{
				ResourceContainers.Add(connector, new(PipeType.PipeProperties.FlowableVolume / Connectors.Length));
			}
		}

		protected override void Tick()
		{
			ResourceContainer lowestFullnessContainer = ResourceContainers.Values.First();

			foreach (var resourceContainer in ResourceContainers.Values)
			{
				if (resourceContainer.Fullness < lowestFullnessContainer.Fullness)
				{
					lowestFullnessContainer = resourceContainer;
				}
			}

			foreach (var resourceContainer in ResourceContainers.Values)
			{
				if (resourceContainer == lowestFullnessContainer)
				{
					continue;
				}

				resourceContainer.TransferResource(lowestFullnessContainer, resourceContainer.Volume * Limiter * (FixedDecimalLong8)Time.TickInterval);
			}
		}

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			return ResourceContainers[pipeConnector];
		}
	}
}
