using FixedPrecision;
using FXRenderer;
using Veldrid;

namespace Space_Refinery_Game
{
	public class PipeConnector : Connector
	{
		public PipeConnector((Pipe connectableA, Pipe connectableB) connectables) : base(connectables)
		{
		}

		public PipeConnector(Pipe initialConnectable, ConnectorSide side) : base(initialConnectable, side)
		{
		}

		public (Pipe? pipeA, Pipe? pipeB) Pipes => ((Pipe? pipeA, Pipe? pipeB))Connectables;

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;
	}
}
