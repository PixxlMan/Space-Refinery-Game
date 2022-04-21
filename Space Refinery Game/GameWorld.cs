using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class GameWorld
	{
		public GameWorld(MainGame mainGame)
		{
			MainGame = mainGame;
		}

		public String SynchronizationObject = "69";

		public HashSet<IConstruction> Constructions = new();

		public MainGame MainGame;

		public HashSet<Entity> Entities = new();

		public void AddEntity(Entity entity)
		{
			lock (SynchronizationObject)
				Entities.Add(entity);
		}

		public void RemoveEntity(Entity entity)
		{
			lock (SynchronizationObject)
				Entities.Remove(entity);
		}

		public void AddConstruction(IConstruction construction)
		{
			lock (SynchronizationObject)
				Constructions.Add(construction);
		}

		public void Deconstruct(IConstruction construction)
		{
			lock (SynchronizationObject)
			{
				Constructions.Remove(construction);

				construction.Deconstruct();
			}
		}

		public static Transform GenerateTransformForConnector(PositionAndDirection chosenConnectorTransform, PipeConnector connector, FixedDecimalLong8 rotation)
		{
			QuaternionFixedDecimalInt4 connectorRotation = /*connector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(connector.Transform.Rotation) :*/ connector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			ITransformable pipeConnectorTransformable = new Transform(connector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Direction : chosenConnectorTransform.Direction;

			Vector3FixedDecimalInt4 position = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Position : chosenConnectorTransform.Position;

			Transform transform =
				new(
					connector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransformable.LocalUnitZ : pipeConnectorTransformable.LocalUnitZ, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransformable.LocalUnitY : pipeConnectorTransformable.LocalUnitY))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransformable.LocalUnitZ, -pipeConnectorTransformable.LocalUnitY), QuaternionFixedDecimalInt4.CreateFromAxisAngle(direction, (FixedDecimalInt4)rotation)))
				);

			transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

			return transform;
		}

		public void StartTicking()
		{
			Thread thread = new Thread(new ThreadStart(() =>
			{
				Stopwatch stopwatch = new();
				stopwatch.Start();

				FixedDecimalInt4 timeLastUpdate = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalInt4>();
				FixedDecimalInt4 time;
				FixedDecimalInt4 deltaTime;
				while (/*MainGame.Running*/true)
				{
					if (!MainGame.Paused)
					{
						time = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalInt4>();

						deltaTime = time - timeLastUpdate;

						timeLastUpdate = time;

						Thread.Sleep((Time.TickInterval * 1000).ToInt32());

						Tick();
					}
				}
			}));

			thread.Start();
		}

		private void Tick()
		{
			lock (SynchronizationObject)
			{
				foreach (var entity in Entities)
				{
					entity.Tick();
				}
			}
		}
	}
}
