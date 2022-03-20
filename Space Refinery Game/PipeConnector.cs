namespace Space_Refinery_Game
{
	public class PipeConnector : Connector
	{
		public PipeConnector((Pipe pipeA, Pipe pipeB) pipes)
		{
			Pipes = pipes;
		}

		public PipeConnector(Pipe initialPipe, ConnectorSide side)
		{
			Pipes = (side == ConnectorSide.A ? (initialPipe, null) : (null, initialPipe));

			VacantSide = (side == ConnectorSide.A ? ConnectorSide.B : ConnectorSide.A);
		}

		public (Pipe? pipeA, Pipe? pipeB) Pipes;

		public PhysicsObject PhysicsObject;

		public Pipe? UnconnectedPipe
		{
			get
			{
				if (!VacantSide.HasValue)
				{
					return null;
				}

				return (VacantSide == ConnectorSide.A ? Pipes.pipeB : Pipes.pipeA);
			}
		}

		public ConnectorSide? VacantSide;

		public ConnectorSide? PopulatedSide
		{
			get
			{
				if (!VacantSide.HasValue)
				{
					return null;
				}

				return (VacantSide == ConnectorSide.A ? ConnectorSide.B : ConnectorSide.A);
			}
		}

		public void Connect(Pipe pipe)
		{
			if (VacantSide == ConnectorSide.A)
			{
				Pipes = (pipe, Pipes.pipeB);

				VacantSide = null;
			}
			else if (VacantSide == ConnectorSide.B)
			{
				Pipes = (Pipes.pipeA, pipe);

				VacantSide = null;
			}
		}

		private IInformationProvider informationProvider = new ConnectorInformationProvider();

		public override IInformationProvider InformationProvider => informationProvider;
	}
}
