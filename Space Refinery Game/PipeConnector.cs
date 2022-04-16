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

		public ConnectorSide ResourceFlowsInto => FlowVelocityIntoA >= 0 ? ConnectorSide.A : ConnectorSide.B;

		public FixedDecimalInt4 FlowVelocityIntoA;

		void Entity.Tick()
		{
			if (Vacant) // should really lead to loss of contents
			{
				return;
			}

			FlowVelocityIntoA = Pipes.pipeA.FlowVelocityTowards(this) - Pipes.pipeB.FlowVelocityTowards(this);

			((Pipe)GetConnectableAtSide(ResourceFlowsInto.Opposite())).ResourceContainer.TransferResource(((Pipe)GetConnectableAtSide(ResourceFlowsInto)).ResourceContainer, (FixedDecimalInt4.PI * (PipeConnectorProperties.ConnectorFlowAreaDiameter / 2) * (PipeConnectorProperties.ConnectorFlowAreaDiameter / 2) * FlowVelocityIntoA)/*cylinder volume*/);

			((Pipe)GetConnectableAtSide(ResourceFlowsInto)).ResourceFlowVelocity += ((Pipe)GetConnectableAtSide(ResourceFlowsInto)).ResourceFlowVelocity;
		}
	}
}
