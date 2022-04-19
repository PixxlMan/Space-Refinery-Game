using FixedPrecision;

namespace Space_Refinery_Game
{
	public class PipeConnector : Connector, Entity
	{
		public PipeConnector((Pipe connectableA, Pipe connectableB) connectables, PipeConnectorProperties pipeConnectorProperties, GameWorld gameWorld) : base(connectables, gameWorld)
		{
			PipeConnectorProperties = pipeConnectorProperties;
			informationProvider = new PipeConnectorInformationProvider(this);
		}

		public PipeConnector(Pipe initialConnectable, ConnectorSide side, PipeConnectorProperties pipeConnectorProperties, GameWorld gameWorld) : base(initialConnectable, side, gameWorld)
		{
			PipeConnectorProperties = pipeConnectorProperties;
			informationProvider = new PipeConnectorInformationProvider(this);
		}

		public PipeConnectorProperties PipeConnectorProperties;

		public (Pipe? pipeA, Pipe? pipeB) Pipes => ((Pipe? pipeA, Pipe? pipeB))Connectables;

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;

		public void TransferResource(Pipe sourcePipe, ResourceContainer sourceContainer, FixedDecimalLong8 volume)
		{
			sourceContainer.TransferResource(((Pipe)GetOther(sourcePipe)).GetResourceContainerForConnector(this), volume);
		}

		/*Entity.SetTickPriority/Frequency(Low)*/
		void Entity.Tick()
		{
			if (Vacant)
			{
				return;
			}

			var pipeAResourceContainer = Pipes.pipeA.GetResourceContainerForConnector(this);
			var pipeBResourceContainer = Pipes.pipeB.GetResourceContainerForConnector(this);

			if (FixedDecimalLong8.Abs(pipeAResourceContainer.Fullness - pipeBResourceContainer.Fullness) > (FixedDecimalLong8)0.0001)
			{
				ConnectorSide flowDirection = pipeAResourceContainer.Fullness - pipeBResourceContainer.Fullness > 0 ? ConnectorSide.B : ConnectorSide.A;

				var recipientContainer = flowDirection == ConnectorSide.A ? pipeAResourceContainer : pipeBResourceContainer;

				var otherContainer = flowDirection == ConnectorSide.A ? pipeBResourceContainer : pipeAResourceContainer;

				var fullnessDifference = FixedDecimalLong8.Abs(recipientContainer.Fullness - otherContainer.Fullness);

				otherContainer.TransferResource(recipientContainer, otherContainer.Volume * fullnessDifference * (FixedDecimalLong8)Time.TickInterval);
			}
		}
	}
}
