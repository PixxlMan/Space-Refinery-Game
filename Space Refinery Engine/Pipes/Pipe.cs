using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using Singulink.Reflection;
using Space_Refinery_Engine;
using Space_Refinery_Game_Renderer;
using System.Xml;
using Veldrid;

namespace Space_Refinery_Engine
{
	// TODO: make thread safe.
	public abstract class Pipe : Entity, IConstruction, IConnectable
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public Transform Transform { get; set; }

		public GraphicsWorld GraphicsWorld;

		public GameWorld GameWorld;

		public MainGame MainGame;

		public PipeConnector[] Connectors;

		protected IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		public PipeType PipeType;

		protected UI UI;

		Connector[] IConnectable.Connectors => Connectors;

		public bool Created;

		public bool Constructed => !Created;

		public SerializableReference SerializableReference { get; private set; } = Guid.NewGuid();

		public SerializationReferenceHandler ReferenceHandler { get; private set; }

		protected Dictionary<string, PipeConnector> NamedConnectors = new();

		private bool destroyed;
		public bool Destroyed
		{
			get
			{
				lock (SyncRoot)
					return destroyed;
			}
			private set
			{
				lock (SyncRoot)
					destroyed = value;
			}
		}

		protected readonly object SyncRoot = new();

		public abstract void TransferResourceFromConnector(ResourceContainer source, VolumeUnit volume, PipeConnector transferingConnector);

		public virtual void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Pipe)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static Pipe Create(PipeType pipeType, Transform transform, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			lock (gameData.Game.GameWorld.TickSyncObject)
			{
				Pipe pipe = (Pipe)ObjectFactory.CreateInstance(pipeType.TypeOfPipe, true);

				pipe.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

				pipeType.BatchRenderable.CreateBatchRenderableEntity(transform, pipe);

				PhysicsObject physObj = CreatePhysicsObject(gameData.PhysicsWorld, transform, pipe, pipeType.Mesh);

				PipeConnector[] connectors = CreateConnectors(pipeType, pipe, gameData);

				pipe.Created = true;

				pipe.SetUp(pipeType, connectors, physObj, gameData);

				gameData.Game.GameWorld.AddEntity(pipe);

				referenceHandler.RegisterReference(pipe);

				return pipe;
			}
		}

		public abstract ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector);

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, Pipe pipeStraight, FXRenderer.Mesh mesh)
		{
			PhysicsObjectDescription<ConvexHull> physicsObjectDescription = new(physWorld.GetConvexHullForMesh(mesh), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);
			return physObj;
		}

		private static PipeConnector[] CreateConnectors(PipeType pipeType, Pipe pipe, GameData gameData)
		{
			PipeConnector[] connectors = new PipeConnector[pipeType.ConnectorPlacements.Length];

			for (int i = 0; i < pipeType.ConnectorPlacements.Length; i++)
			{
				MainGame.DebugRender.PersistentCube(
					new(
						pipe.Transform.Position +
						Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation)
						, pipe.Transform.Rotation
						),
					new RgbaFloat(i / (float)pipeType.ConnectorPlacements.Length, i / 10f + .1f, 0, 1),
					new((DN).125, (DN).125, (DN).125)
					);

				// Connect with existing connector at this location - forgo creation of own connector.
				if (gameData.PhysicsWorld.ApproxOverlapPoint<PipeConnector>(pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation), out PhysicsObject physicsObject))
				{
					PipeConnector pipeConnector = (PipeConnector)physicsObject.Entity;

					pipeConnector.Connect(pipe);

					connectors[i] = pipeConnector;

					if (pipeType.ConnectorNames is not null && pipeType.ConnectorNames[i] is not null)
					{
						pipe.NamedConnectors.Add(pipeType.ConnectorNames[i], pipeConnector);
					}

					continue;
				}
				// Create own connector if there is no connector to connect to.
				else
				{
					QuaternionFixedDecimalInt4 rotation =
						QuaternionFixedDecimalInt4.Concatenate(
							QuaternionFixedDecimalInt4.CreateLookingAt(
								pipeType.ConnectorPlacements[i].Direction,
								Vector3FixedDecimalInt4.UnitZ,
								Vector3FixedDecimalInt4.UnitY),
							pipe.Transform.Rotation);

					Vector3FixedDecimalInt4 position = pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation);

					Transform transform = new(
						position,
						rotation
					);
					transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

					PipeConnector connector = new PipeConnector(pipe, ConnectorSide.A, transform, pipeType.ConnectorProperties[i], gameData);

					gameData.Game.GameWorld.AddEntity(connector);

					gameData.Game.GameReferenceHandler.RegisterReference(connector);

					connectors[i] = connector;

					if (pipeType.ConnectorNames is not null && pipeType.ConnectorNames[i] is not null)
					{
						pipe.NamedConnectors.Add(pipeType.ConnectorNames[i], connector);
					}

					continue;
				}
			}

			return connectors;
		}

		private static bool ValidateConnectors(PipeType pipeType, Transform transform, GameData gameData)
		{
			for (int i = 0; i < pipeType.ConnectorPlacements.Length; i++)
			{
				//MainGame.DebugRender.PersistentCube(new (transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, transform.Rotation), transform.Rotation, new((DecimalNumber).125, (DecimalNumber).125, (DecimalNumber).125)), new RgbaFloat((float)i / (float)pipeType.ConnectorPlacements.Length, (float)i / 10f + .1f, 0, 1));

				if (gameData.PhysicsWorld.ApproxOverlapPoint<PipeConnector>(transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, transform.Rotation), out PhysicsObject physicsObject))
				{
					PipeConnector pipeConnector = (PipeConnector)physicsObject.Entity;

					if (!pipeConnector.Vacant)
					{
						return false;
					}
				}
			}

			return true;
		}

		public static bool ValidateBuild(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, GameData gameData)
		{
			lock (gameData.Game.GameWorld.TickSyncObject)
			{
				Transform transform = Connector.GenerateTransformForConnector(((PipeType)entityType).ConnectorPlacements[indexOfSelectedConnector], connector, rotation);

				if (connector is not PipeConnector ||
					entityType is not Space_Refinery_Engine.PipeType ||
					//!ValidatePhysics(gameData.PhysicsWorld, transform, pipe, pipeType.Mesh)||
					!ValidateConnectors((PipeType)entityType, transform, gameData))
				{
					return false;
				}
			}

			return true;
		}

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			lock (gameData.Game.GameWorld.TickSyncObject)
			{
				PipeConnector pipeConnector = (PipeConnector)connector;

				PipeType pipeType = (PipeType)entityType;

				Transform transform = Connector.GenerateTransformForConnector(pipeType.ConnectorPlacements[indexOfSelectedConnector], pipeConnector, rotation);

				Pipe pipe = (Pipe)ObjectFactory.CreateInstance(pipeType.TypeOfPipe, true);

				pipe.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

				pipeType.BatchRenderable.CreateBatchRenderableEntity(transform, pipe);

				PhysicsObject physObj = CreatePhysicsObject(gameData.PhysicsWorld, transform, pipe, pipeType.Mesh);

				var connectors = CreateConnectors(pipeType, pipe, gameData);

				pipe.SetUp(pipeType, connectors, physObj, gameData);

				pipe.Created = false;

				gameData.Game.GameWorld.AddEntity(pipe);

				referenceHandler.RegisterReference(pipe);

				return pipe;
			}
		}

		private void SetUp(PipeType pipeType, PipeConnector[] connectors, PhysicsObject physicsObject, GameData gameData)
		{
			lock (SyncRoot)
			{
				PipeType = pipeType;
				UI = gameData.UI;
				PhysicsWorld = gameData.PhysicsWorld;
				PhysicsObject = physicsObject;
				Connectors = connectors;
				GraphicsWorld = gameData.GraphicsWorld;
				GameWorld = gameData.Game.GameWorld;
				MainGame = gameData.MainGame;
				ReferenceHandler = gameData.Game.GameReferenceHandler;

				SetUp();
			}
		}

		protected virtual void SetUp()
		{

		}

		public virtual void Deconstruct()
		{
			lock (SyncRoot)
			{
				if (Destroyed)
				{
					return;
				}

				DisplaceContents();

				Destroy();
			}
		}

		protected virtual void DisplaceContents()
		{
		}

		public virtual void Tick()
		{
		}

		void Entity.Interacted()
		{
			Interacted();
		}

		protected virtual void Interacted()
		{
		}

		public virtual void SerializeState(XmlWriter writer)
		{
			lock (SyncRoot)
			{
				writer.WriteStartElement(nameof(Pipe));
				{
					writer.SerializeReference(this);

					writer.WriteElementString("PipeType", PipeType.Name);

					writer.Serialize(Transform);

					writer.Serialize(Connectors, (w, c) => w.SerializeReference(c), nameof(Connectors));
				}
				writer.WriteEndElement();
			}
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			lock (SyncRoot)
			{
				reader.ReadStartElement(nameof(Pipe));
				{
					SerializableReference = reader.ReadReference();

					PipeType pipeType = PipeType.PipeTypes[reader.ReadElementString("PipeType")];

					Transform transform = reader.DeserializeTransform();

					SetupDeserialized(pipeType, transform, serializationData);
				}
				reader.ReadEndElement();
			}

			void SetupDeserialized(PipeType pipeType, Transform transform, SerializationData serializationData)
			{
				Transform = transform;

				MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

				pipeType.BatchRenderable.CreateBatchRenderableEntity(transform, this);

				PhysicsObject physObj = CreatePhysicsObject(serializationData.GameData.PhysicsWorld, transform, this, pipeType.Mesh);

				PipeConnector[] connectors = (PipeConnector[])reader.DeserializeCollection<PipeConnector>((r, setAction, i) => r.DeserializeReference(referenceHandler,
					(s) =>
					{
						if (pipeType.ConnectorNames is not null && pipeType.ConnectorNames[i] is not null)
						{
							NamedConnectors.Add(pipeType.ConnectorNames[i], (PipeConnector)s);
						}

						setAction((PipeConnector)s);
					}), nameof(Connectors));

				PipeType = pipeType;

				serializationData.DeserializationCompleteEvent += () =>
				{
					SetUp(pipeType, connectors, physObj, serializationData.GameData);
				};

				Created = true;

				serializationData.GameData.Game.GameWorld.AddEntity(this);
			}
		}

		public virtual void Destroy()
		{
			lock (SyncRoot)
			{
				if (Destroyed)
				{
					return;
				}

				Destroyed = true;

				PhysicsObject.Destroy();
				PipeType.BatchRenderable.RemoveBatchRenderableEntity(this);

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

				foreach (var connector in Connectors)
				{
					connector.Disconnect(this);
				}

				ReferenceHandler.RemoveReference(this);
			}
		}
	}
}
