using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class PipeConnector : Connector, Entity
	{
		private PipeConnector()
		{
		}

		public PipeConnector((Pipe connectableA, Pipe connectableB) connectables, Transform transform, PipeConnectorProperties pipeConnectorProperties, GameData gameData) : base(connectables, transform, gameData)
		{
			PipeConnectorProperties = pipeConnectorProperties;
		}

		public PipeConnector(Pipe initialConnectable, ConnectorSide side, Transform transform, PipeConnectorProperties pipeConnectorProperties, GameData gameData) : base(initialConnectable, side, transform, gameData)
		{
			PipeConnectorProperties = pipeConnectorProperties;
		}

		protected override void SetUp()
		{
			base.SetUp();

			informationProvider = new PipeConnectorInformationProvider(this);
		}

		public PipeConnectorProperties PipeConnectorProperties;

		public (Pipe? pipeA, Pipe? pipeB) Pipes => ((Pipe? pipeA, Pipe? pipeB))Connectables;

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;

		public void TransferResource(Pipe sourcePipe, ResourceContainer sourceContainer, VolumeUnit volume)
		{
			lock (this)
			{
				sourceContainer.TransferAllResource(((Pipe)GetOther(sourcePipe)).GetResourceContainerForConnector(this));
			}
		}

		/*Entity.SetTickPriority/Frequency(Low)*/
		public override void Tick()
		{
			lock (this)
			{
				if (Vacant)
				{
					return;
				}

				var pipeAResourceContainer = Pipes.pipeA.GetResourceContainerForConnector(this);
				var pipeBResourceContainer = Pipes.pipeB.GetResourceContainerForConnector(this);

				if (DecimalNumber.Difference(pipeAResourceContainer.Fullness, pipeBResourceContainer.Fullness) != 0)
				{
					ConnectorSide flowDirection = pipeAResourceContainer.Fullness - pipeBResourceContainer.Fullness > 0 ? ConnectorSide.B : ConnectorSide.A;

					var recipientContainer = flowDirection == ConnectorSide.A ? pipeAResourceContainer : pipeBResourceContainer;

					var otherContainer = flowDirection == ConnectorSide.A ? pipeBResourceContainer : pipeAResourceContainer;

					var fullnessDifference = DecimalNumber.Abs(recipientContainer.Fullness - otherContainer.Fullness);

					otherContainer.TransferResourceByVolume(recipientContainer, otherContainer.Volume * fullnessDifference * (DecimalNumber)Time.TickInterval);
				}
			}
		}

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.SerializeReference(PipeConnectorProperties, nameof(PipeConnectorProperties));
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			reader.DeserializeReference<PipeConnectorProperties>(MainGame.GlobalReferenceHandler, (p) => PipeConnectorProperties = p, nameof(PipeConnectorProperties));
		}
	}
}
