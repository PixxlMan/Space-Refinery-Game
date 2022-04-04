using FixedPrecision;
using FXRenderer;
using Veldrid;

namespace Space_Refinery_Game
{
	public class PipeConnector : Connector
	{
		public PipeConnector((Pipe pipeA, Pipe pipeB) pipes) : this()
		{
			Pipes = pipes;
		}

		public PipeConnector(Pipe initialPipe, ConnectorSide side) : this()
		{
			Pipes = (side == ConnectorSide.A ? (initialPipe, null) : (null, initialPipe));

			VacantSide = (side == ConnectorSide.A ? ConnectorSide.B : ConnectorSide.A);
		}

		private PipeConnector()
		{
			informationProvider = new PipeConnectorInformationProvider(this);

			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;
		}

		public void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanSetting>($"{nameof(PipeConnector)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(PhysicsObject.Transform);

			MainGame.DebugRender.DrawCube(new (PhysicsObject.Transform) { Scale = new((FixedDecimalInt4).4f, (FixedDecimalInt4).4f, (FixedDecimalInt4).25f)}, VacantSide is null ? RgbaFloat.Green : RgbaFloat.Cyan);
		}

		public (Pipe? pipeA, Pipe? pipeB) Pipes;

		public PhysicsObject PhysicsObject;

		public ConnectorProxy Proxy;

		public Transform Transform;

		public bool Vacant => VacantSide is not null;

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
			if (!Vacant)
			{
				throw new Exception("PipeConnector is not vacant.");
			}

			if (VacantSide == ConnectorSide.A)
			{
				Pipes = (pipe, Pipes.pipeB);

				if (Pipes.pipeB is not null)
				{
					VacantSide = null;
				}
			}
			else if (VacantSide == ConnectorSide.B)
			{
				Pipes = (Pipes.pipeA, pipe);

				if (Pipes.pipeA is not null)
				{
					VacantSide = null;
				}
			}
		}

		public void Disconnect(ConnectorSide side)
		{
			if (side == ConnectorSide.A)
			{
				Pipes = (null, Pipes.pipeB);
			}
			else if (side == ConnectorSide.B)
			{
				Pipes = (Pipes.pipeA, null);
			}
			VacantSide = side;

			if (Pipes.pipeA is null && Pipes.pipeB is null)
			{
				PhysicsObject.Destroy();

				Proxy.PhysicsObject.Destroy();

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;
			}
		}

		public void Disconnect(Pipe pipe)
		{
			if (Pipes.pipeA == pipe)
			{
				Disconnect(ConnectorSide.A);
			}
			else if (Pipes.pipeB == pipe)
			{
				Disconnect(ConnectorSide.B);
			}
		}

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;
	}
}
