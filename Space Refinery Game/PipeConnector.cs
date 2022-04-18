using FixedPrecision;
using FXRenderer;
using Veldrid;

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

		void Entity.Tick()
		{
			if (Vacant)
			{
				return;
			}

			if (FixedDecimalLong8.Abs(Pipes.pipeA.Fullness - Pipes.pipeB.Fullness) > (FixedDecimalLong8)0.0001)
			{
				ConnectorSide flowDirection = Pipes.pipeA.Fullness - Pipes.pipeB.Fullness > 0 ? ConnectorSide.B : ConnectorSide.A;

				Pipe recipientPipe = (Pipe)GetConnectableAtSide(flowDirection);

				Pipe otherPipe = (Pipe)GetConnectableAtSide(flowDirection.Opposite());

				var fullnessDifference = FixedDecimalLong8.Abs(recipientPipe.Fullness - otherPipe.Fullness);

				otherPipe.ResourceContainer.TransferResource(recipientPipe.ResourceContainer, otherPipe.ResourceContainer.GetVolume() * fullnessDifference * (FixedDecimalLong8)Time.TickInterval);
			}
		}
	}
}
