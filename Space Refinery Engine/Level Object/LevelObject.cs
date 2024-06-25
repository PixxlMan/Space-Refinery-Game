using Space_Refinery_Game_Renderer;
using Singulink.Reflection;
using System.Xml;

namespace Space_Refinery_Engine
{
	// TODO: make thread safe.
	public abstract class LevelObject : Entity
	{
		public PhysicsObject? PhysicsObject { get; private set; }

		private Transform transform;
		public Transform Transform
		{
			get => transform;
			set
			{
				if (PhysicsObject is not null)
				{
					PhysicsObject.Transform = value;
				}
				transform = value;
			}
		}

		protected IInformationProvider? informationProvider;

		public IInformationProvider? InformationProvider => informationProvider;

		public LevelObjectType LevelObjectType { get; private set; }

		public SerializableReference SerializableReference { get; private set; } = Guid.NewGuid();

		public SerializationReferenceHandler ReferenceHandler { get; private set; }

		public string Name => LevelObjectType.Name;

		protected GameData gameData;

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
			if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(LevelObject)} collider debug"))
			{
				LevelObjectType.Collider.AddDebugObjects(transform);
			}

			if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(LevelObject)} debug orientation marks"))
			{
				GameData.DebugRender.DrawOrientationMarks(Transform);
			}
		}

		public static LevelObject Create(LevelObjectType levelObjectType, Transform transform, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			lock (gameData.Game.GameWorld.TickSyncObject)
			{
				LevelObject levelObject = (LevelObject)ObjectFactory.CreateInstance(levelObjectType.TypeOfLevelObject, true);

				levelObject.Transform = transform;

				levelObject.gameData = gameData;

				GameData.DebugRender.AddDebugObjects += levelObject.AddDebugObjects;

				levelObjectType.BatchRenderable.CreateBatchRenderableEntity(transform, levelObject);

				levelObject.SetUp(levelObjectType, gameData);

				gameData.Game.GameWorld.AddEntity(levelObject);

				referenceHandler.RegisterReference(levelObject);

				return levelObject;
			}
		}

		protected abstract PhysicsObject? CreatePhysicsObject();

		private void SetUp(LevelObjectType levelObjectType, GameData gameData)
		{
			lock (SyncRoot)
			{
				LevelObjectType = levelObjectType;
				ReferenceHandler = gameData.Game.GameReferenceHandler;

				PhysicsObject = CreatePhysicsObject();

				SetUp();
			}
		}

		protected virtual void SetUp()
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
				writer.WriteStartElement(nameof(LevelObject));
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
				reader.ReadStartElement(nameof(LevelObject));
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

				GameData.DebugRender.AddDebugObjects += AddDebugObjects;

				levelObjectType.BatchRenderable.CreateBatchRenderableEntity(transform, this);

				LevelObjectType = levelObjectType;

				serializationData.DeserializationCompleteEvent += () =>
				{
					SetUp(levelObjectType, serializationData.GameData);
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

				PhysicsObject?.Destroy();

				LevelObjectType.BatchRenderable.RemoveBatchRenderableEntity(this);

				GameData.DebugRender.AddDebugObjects -= AddDebugObjects;

				ReferenceHandler.RemoveReference(this);
			}
		}
	}
}
