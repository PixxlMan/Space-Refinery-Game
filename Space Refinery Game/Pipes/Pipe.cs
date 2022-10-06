using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Veldrid;

namespace Space_Refinery_Game
{
	public abstract class Pipe : Entity, IConstruction, IConnectable
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public Transform Transform { get; set; }

		public GraphicsWorld GraphicsWorld;

		public GameWorld GameWorld;

		public MainGame MainGame;

		public EntityRenderable Renderable;
		
		public PipeConnector[] Connectors;

		protected IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		public PipeType PipeType;

		protected UI UI;

		Connector[] IConnectable.Connectors => Connectors;

		public ConstructionInfo? ConstructionInfo { get; private set; }

		public bool Created;

		public bool Constructed => !Created;

		public Guid SerializableReferenceGUID { get; private set; } = Guid.NewGuid();

		public SerializationReferenceHandler ReferenceHandler { get; private set; }

		protected Dictionary<string, PipeConnector> NamedConnectors = new();

		public bool Destroyed { get; private set; }

		public abstract void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, PipeConnector transferingConnector);

		public virtual void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Pipe)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static Pipe Create(PipeType pipeType, Transform transform, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			lock (gameData.GameWorld.TickSyncObject)
			{
				Pipe pipe = (Pipe)Activator.CreateInstance(pipeType.TypeOfPipe, true);

				pipe.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

				EntityRenderable renderable = CreateRenderable(pipeType, gameData.GraphicsWorld, transform);

				PhysicsObject physObj = CreatePhysicsObject(gameData.PhysicsWorld, transform, pipe, pipeType.Mesh);

				PipeConnector[] connectors = CreateConnectors(pipeType, pipe, gameData);

				pipe.Created = true;

				pipe.SetUp(pipeType, null, connectors, renderable, physObj, gameData);

				gameData.GameWorld.AddEntity(pipe);

				gameData.GameWorld.AddConstruction(pipe);

				referenceHandler.RegisterReference(pipe);

				return pipe;
			}
		}

		public abstract ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector);

		private static EntityRenderable CreateRenderable(PipeType pipeType, GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld, transform, pipeType.Mesh, Utils.GetSolidColoredTexture(RgbaByte.LightGrey, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			return renderable;
		}

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
				MainGame.DebugRender.PersistentCube(new (pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation), pipe.Transform.Rotation, new((FixedDecimalInt4).125, (FixedDecimalInt4).125, (FixedDecimalInt4).125)), new RgbaFloat((float)i / (float)pipeType.ConnectorPlacements.Length, (float)i / 10f + .1f, 0, 1));
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

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			lock (gameData.GameWorld.TickSyncObject)
			{
				PipeConnector pipeConnector = (PipeConnector)connector;

				PipeType pipeType = (PipeType)entityType;

				Transform transform = GameWorld.GenerateTransformForConnector(pipeType.ConnectorPlacements[indexOfSelectedConnector], pipeConnector, rotation);

				Pipe pipe = (Pipe)Activator.CreateInstance(pipeType.TypeOfPipe, true);

				pipe.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

				EntityRenderable renderable = CreateRenderable(pipeType, gameData.GraphicsWorld, transform);

				PhysicsObject physObj = CreatePhysicsObject(gameData.PhysicsWorld, transform, pipe, pipeType.Mesh);

				var connectors = CreateConnectors(pipeType, pipe, gameData);

				pipe.SetUp(pipeType, new(indexOfSelectedConnector, rotation), connectors, renderable, physObj, gameData);

				pipe.Created = false;

				gameData.GameWorld.AddEntity(pipe);

				gameData.GameWorld.AddConstruction(pipe);

				referenceHandler.RegisterReference(pipe);

				return pipe;
			}
		}

		private void SetUp(PipeType pipeType, ConstructionInfo? constructionInfo, PipeConnector[] connectors, EntityRenderable renderable, PhysicsObject physicsObject, GameData gameData)
		{
			lock (this)
			{
				PipeType = pipeType;
				ConstructionInfo = constructionInfo;
				UI = gameData.UI;
				PhysicsWorld = gameData.PhysicsWorld;
				PhysicsObject = physicsObject;
				Connectors = connectors;
				GraphicsWorld = gameData.GraphicsWorld;
				Renderable = renderable;
				GameWorld = gameData.GameWorld;
				MainGame = gameData.MainGame;
				ReferenceHandler = gameData.ReferenceHandler;

				SetUp();
			}
		}

		protected virtual void SetUp()
		{

		}

		public virtual void Deconstruct()
		{
			lock (this)
			{
				if (Destroyed)
				{
					return;
				}

				Destroyed = true;

				DisplaceContents();

				PhysicsObject.Destroy();
				Renderable.Destroy();

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

				foreach (var connector in Connectors)
				{
					connector.Disconnect(this);
				}

				ReferenceHandler.RemoveReference(this);
			}
		}

		protected virtual void DisplaceContents()
		{
		}

		public virtual void Tick() { }

		void Entity.Interacted()
		{
			Interacted();
		}

		protected virtual void Interacted()
		{ 
		}

		public virtual void SerializeState(XmlWriter writer)
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

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(Pipe));
			{
				SerializableReferenceGUID = reader.ReadReferenceGUID();

				PipeType pipeType = PipeType.PipeTypes[reader.ReadElementString("PipeType")];

				Transform transform = reader.DeserializeTransform();

				SetupDeserialized(pipeType, transform, serializationData);
			}
			reader.ReadEndElement();

			void SetupDeserialized(PipeType pipeType, Transform transform, SerializationData serializationData)
			{
				Transform = transform;

				MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

				EntityRenderable renderable = CreateRenderable(pipeType, serializationData.GameData.GraphicsWorld, transform);

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

				serializationData.SerializationCompleteEvent += () =>
				{
					SetUp(pipeType, null, connectors, renderable, physObj, serializationData.GameData);
				};

				Created = true;

				serializationData.GameData.GameWorld.AddEntity(this);

				serializationData.GameData.GameWorld.AddConstruction(this);
			}
		}

		void Entity.Destroy() => Deconstruct();
	}
}
