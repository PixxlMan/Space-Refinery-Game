using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;

namespace Space_Refinery_Game
{
	public class PipeConnector : Connector, Entity
	{
		public PipeConnector((Pipe connectableA, Pipe connectableB) connectables, Transform transform, PipeConnectorProperties pipeConnectorProperties, GameWorld gameWorld, PhysicsWorld physicsWorld, UI ui) : base(connectables, transform, gameWorld, physicsWorld, ui)
		{
			PipeConnectorProperties = pipeConnectorProperties;
			informationProvider = new PipeConnectorInformationProvider(this);
			CreatePhysicsObject(transform, physicsWorld);
		}

		public PipeConnector(Pipe initialConnectable, ConnectorSide side, Transform transform, PipeConnectorProperties pipeConnectorProperties, GameWorld gameWorld, PhysicsWorld physicsWorld, UI ui) : base(initialConnectable, side, transform, gameWorld, physicsWorld, ui)
		{
			PipeConnectorProperties = pipeConnectorProperties;
			informationProvider = new PipeConnectorInformationProvider(this);
			CreatePhysicsObject(transform, physicsWorld);
		}

		private void CreatePhysicsObject(Transform transform, PhysicsWorld physicsWorld)
		{
			var physicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(.1f, .1f, .1f), transform, 0, true);

			PhysicsObject = physicsWorld.AddPhysicsObject(physicsObjectDescription, this);
		}

		public PipeConnectorProperties PipeConnectorProperties;

		public (Pipe? pipeA, Pipe? pipeB) Pipes => ((Pipe? pipeA, Pipe? pipeB))Connectables;

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;

		public void TransferResource(Pipe sourcePipe, ResourceContainer sourceContainer, FixedDecimalLong8 volume)
		{
			lock (this)
			{
				sourceContainer.TransferResource(((Pipe)GetOther(sourcePipe)).GetResourceContainerForConnector(this), volume);
			}
		}

		/*Entity.SetTickPriority/Frequency(Low)*/
		void Entity.Tick()
		{
			lock (this)
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
}
