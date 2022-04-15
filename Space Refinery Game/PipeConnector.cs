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
		}

		public PipeConnector(Pipe initialConnectable, ConnectorSide side, PipeConnectorProperties pipeConnectorProperties, GameWorld gameWorld) : base(initialConnectable, side, gameWorld)
		{
			PipeConnectorProperties = pipeConnectorProperties;
		}

		public PipeConnectorProperties PipeConnectorProperties;

		public (Pipe? pipeA, Pipe? pipeB) Pipes => ((Pipe? pipeA, Pipe? pipeB))Connectables;

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;

		public ConnectorSide ResourceFlowsInto;

		void Entity.Tick()
		{

		}
	}
}
