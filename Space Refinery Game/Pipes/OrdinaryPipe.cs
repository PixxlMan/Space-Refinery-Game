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
using System.Xml;
using Veldrid;

namespace Space_Refinery_Game
{
	public sealed class OrdinaryPipe : Pipe
	{
		private OrdinaryPipe()
		{
			informationProvider = new OrdinaryPipeInformationProvider(this);
		}

		public ResourceContainer ResourceContainer;

		public override void TransferResourceFromConnector(ResourceContainer source, DecimalNumber volume, PipeConnector _)
		{
			lock (this)
			{
				ResourceContainer.TransferResourceByVolume(source, volume);
			}
		}

		protected override void DisplaceContents()
		{
			lock (this)
			{
				List<PipeConnector> connectedConnectors = new();
				foreach (var connector in Connectors)
				{
					if (!connector.Vacant)
						connectedConnectors.Add(connector);
				}

				if (connectedConnectors.Count == 0)
				{
					return;
				}

				var volumePerConnector = ResourceContainer.Volume / connectedConnectors.Count;

				foreach (var connectedConnector in connectedConnectors)
				{
					connectedConnector.TransferResource(this, ResourceContainer, volumePerConnector);
				}
			}
		}

		protected override void SetUp()
		{
			lock (this)
			{
				ResourceContainer ??= new(PipeType.PipeProperties.FlowableVolume);
			}
		}

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			return ResourceContainer;
		}

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			ResourceContainer.Serialize(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			ResourceContainer = ResourceContainer.Deserialize(reader);
		}
	}
}
