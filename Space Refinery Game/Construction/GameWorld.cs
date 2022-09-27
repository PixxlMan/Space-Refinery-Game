using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public class GameWorld
	{
		public GameWorld(MainGame mainGame)
		{
			MainGame = mainGame;
		}

		public object SynchronizationObject = new();

		public object TickSyncObject = new();

		public SerializationReferenceHandler SerializationReferenceHandler { get; private set; } = new();

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

		public static Transform GenerateTransformForConnector(PositionAndDirection chosenConnectorTransform, Connector connector, FixedDecimalLong8 rotation)
		{
			QuaternionFixedDecimalInt4 connectorRotation = /*connector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(connector.Transform.Rotation) :*/ connector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			Transform pipeConnectorTransform = new Transform(connector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Direction : chosenConnectorTransform.Direction;

			Vector3FixedDecimalInt4 position = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Position : chosenConnectorTransform.Position;

			Transform transform =
				new(
					connector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransform.LocalUnitZ : pipeConnectorTransform.LocalUnitZ, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransform.LocalUnitY : pipeConnectorTransform.LocalUnitY))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransform.LocalUnitZ, -pipeConnectorTransform.LocalUnitY), QuaternionFixedDecimalInt4.CreateFromAxisAngle(direction, (FixedDecimalInt4)rotation)))
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
			}))
			{ Name = "Tick Update Thread" };

			thread.Start();
		}

		private void Tick()
		{
			lock (TickSyncObject) lock (SynchronizationObject)
			{
				foreach (var entity in Entities)
				{
					entity.Tick();
				}
			}
		}

		public void SerializeConstructions(XmlWriter writer)
		{
			lock (SynchronizationObject)
			{
				writer.WriteStartElement("GameWorld");
				{
					writer.Serialize(Constructions, (w, c) => c.Serialize(w), "Constructions");
				}
				writer.WriteEndElement();
			}
		}

		public void DeserializeConstructions(XmlReader reader, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, SerializationReferenceHandler referenceHandler)
		{
			lock (SynchronizationObject)
			{
				foreach (var construction in Constructions.ToArray())
				{
					Deconstruct(construction);
				}

				reader.ReadStartElement("GameWorld");
				{
					reader.DeserializeCollection<IConstruction>((r) =>
					{
						IConstructionSerialization.Deserialize(reader, ui, physicsWorld, graphicsWorld, this, MainGame, referenceHandler);

						return null;
					}, "Constructions");
				}
				reader.ReadEndElement();
			}
		}
	}
}
