using BepuPhysics.Collidables;
using Space_Refinery_Game_Renderer;
using Singulink.Reflection;
using System.Xml;

namespace Space_Refinery_Engine
{
	// TODO: make thread safe.
	public abstract class LevelObject : Entity
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public Transform Transform { get; set; }

		public GraphicsWorld GraphicsWorld;

		public GameWorld GameWorld;

		public MainGame MainGame;

		protected IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		public LevelObjectType LevelObjectType;

		protected UI UI;

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

		public virtual void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Pipe)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static LevelObject Create(LevelObjectType levelObjectType, Transform transform, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			lock (gameData.Game.GameWorld.TickSyncObject)
			{
				LevelObject levelObject = (LevelObject)ObjectFactory.CreateInstance(levelObjectType.TypeOfPipe, true);

				levelObject.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += levelObject.AddDebugObjects;

				levelObjectType.BatchRenderable.CreateBatchRenderableEntity(transform, levelObject);

				PhysicsObject physObj = CreatePhysicsObject(gameData.PhysicsWorld, transform, levelObject, levelObjectType.Mesh);

				levelObject.SetUp(levelObjectType, physObj, gameData);

				gameData.Game.GameWorld.AddEntity(levelObject);

				referenceHandler.RegisterReference(levelObject);

				return levelObject;
			}
		}

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, LevelObject levelObject, Space_Refinery_Game_Renderer.Mesh mesh)
		{
			PhysicsObjectDescription<ConvexHull> physicsObjectDescription = new(physWorld.GetConvexHullForMesh(mesh), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, levelObject);
			return physObj;
		}

		private void SetUp(LevelObjectType levelObjectType, PhysicsObject physicsObject, GameData gameData)
		{
			lock (SyncRoot)
			{
				UI = gameData.UI;
				PhysicsWorld = gameData.PhysicsWorld;
				PhysicsObject = physicsObject;
				GraphicsWorld = gameData.GraphicsWorld;
				GameWorld = gameData.Game.GameWorld;
				MainGame = gameData.MainGame;
				LevelObjectType = levelObjectType;
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

					writer.WriteElementString("LevelObjectType", LevelObjectType.Name);

					writer.Serialize(Transform);
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

					LevelObjectType levelObjectType = LevelObjectType.LevelObjectTypes[reader.ReadElementString("LevelObjectType")];

					Transform transform = reader.DeserializeTransform();

					SetupDeserialized(levelObjectType, transform, serializationData);
				}
				reader.ReadEndElement();
			}

			void SetupDeserialized(LevelObjectType levelObjectType, Transform transform, SerializationData serializationData)
			{
				Transform = transform;

				MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

				levelObjectType.BatchRenderable.CreateBatchRenderableEntity(transform, this);

				PhysicsObject physObj = CreatePhysicsObject(serializationData.GameData.PhysicsWorld, transform, this, levelObjectType.Mesh);

				LevelObjectType = levelObjectType;

				serializationData.DeserializationCompleteEvent += () =>
				{
					SetUp(levelObjectType, physObj, serializationData.GameData);
				};

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

				LevelObjectType.BatchRenderable.RemoveBatchRenderableEntity(this);

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

				ReferenceHandler.RemoveReference(this);
			}
		}
	}
}
