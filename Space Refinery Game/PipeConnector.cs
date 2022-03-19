namespace Space_Refinery_Game
{
	public class PipeConnector : Connector
	{
		public (Pipe pipeA, Pipe pipeB) Pipes;

		private IInformationProvider informationProvider = new ConnectorInformationProvider();

		public override IInformationProvider InformationProvider => informationProvider;
	}
}
