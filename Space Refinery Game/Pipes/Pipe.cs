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

		protected Dictionary<string, PipeConnector> NamedConnectors = new();

		public abstract void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, PipeConnector transferingConnector);

		public virtual void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Pipe)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static Pipe Create(PipeType pipeType, Transform transform, UI ui, PhysicsWorld physWorld, GraphicsWorld graphWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			lock (gameWorld.TickSyncObject)
			{
				Pipe pipe = (Pipe)Activator.CreateInstance(pipeType.TypeOfPipe, true);

				pipe.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

				EntityRenderable renderable = CreateRenderable(pipeType, graphWorld, transform);

				PhysicsObject physObj = CreatePhysicsObject(physWorld, transform, pipe, pipeType.Mesh);

				PipeConnector[] connectors = CreateConnectors(pipeType, pipe, physWorld, gameWorld, ui);

				pipe.Created = true;

				pipe.SetUp(pipeType, null, ui, physWorld, physObj, connectors, graphWorld, renderable, gameWorld, mainGame);

				gameWorld.AddEntity(pipe);

				gameWorld.AddConstruction(pipe);

				referenceHandler.RegisterReference(pipe);

				return pipe;
			}
		}

		public abstract ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector);

		private static EntityRenderable CreateRenderable(PipeType pipeType, GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, pipeType.Mesh, Utils.GetSolidColoredTexture(RgbaByte.LightGrey, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);

			return renderable;
		}

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, Pipe pipeStraight, FXRenderer.Mesh mesh)
		{
			PhysicsObjectDescription<ConvexHull> physicsObjectDescription = new(physWorld.GetConvexHullForMesh(mesh), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);
			return physObj;
		}

		private static PipeConnector[] CreateConnectors(PipeType pipeType, Pipe pipe, PhysicsWorld physWorld, GameWorld gameWorld, UI ui)
		{
			PipeConnector[] connectors = new PipeConnector[pipeType.ConnectorPlacements.Length];

			for (int i = 0; i < pipeType.ConnectorPlacements.Length; i++)
			{
				MainGame.DebugRender.PersistentCube(new (pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation), pipe.Transform.Rotation, new((FixedDecimalInt4).125, (FixedDecimalInt4).125, (FixedDecimalInt4).125)), new RgbaFloat((float)i / (float)pipeType.ConnectorPlacements.Length, (float)i / 10f + .1f, 0, 1));
				if (physWorld.ApproxOverlapPoint<PipeConnector>(pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation), out PhysicsObject physicsObject))
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

					PipeConnector connector = new PipeConnector(pipe, ConnectorSide.A, transform, pipeType.ConnectorProperties[i], gameWorld, physWorld, ui);

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

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			lock (gameWorld.TickSyncObject)
			{
				PipeConnector pipeConnector = (PipeConnector)connector;

				PipeType pipeType = (PipeType)entityType;

				Transform transform = GameWorld.GenerateTransformForConnector(pipeType.ConnectorPlacements[indexOfSelectedConnector], pipeConnector, rotation);

				Pipe pipe = (Pipe)Activator.CreateInstance(pipeType.TypeOfPipe, true);

				pipe.Transform = transform;

				MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

				EntityRenderable renderable = CreateRenderable(pipeType, graphicsWorld, transform);

				PhysicsObject physObj = CreatePhysicsObject(physicsWorld, transform, pipe, pipeType.Mesh);

				var connectors = CreateConnectors(pipeType, pipe, physicsWorld, gameWorld, ui);

				pipe.SetUp(pipeType, new(indexOfSelectedConnector, rotation), ui, physicsWorld, physObj, connectors, graphicsWorld, renderable, gameWorld, mainGame);

				pipe.Created = false;

				gameWorld.AddEntity(pipe);

				gameWorld.AddConstruction(pipe);

				referenceHandler.RegisterReference(pipe);

				return pipe;
			}
		}

		private void SetUp(PipeType pipeType, ConstructionInfo? constructionInfo, UI ui, PhysicsWorld physicsWorld, PhysicsObject physicsObject, PipeConnector[] connectors, GraphicsWorld graphicsWorld, EntityRenderable renderable, GameWorld gameWorld, MainGame mainGame)
		{
			lock (this)
			{
				PipeType = pipeType;
				ConstructionInfo = constructionInfo;
				UI = ui;
				PhysicsWorld = physicsWorld;
				PhysicsObject = physicsObject;
				Connectors = connectors;
				GraphicsWorld = graphicsWorld;
				Renderable = renderable;
				GameWorld = gameWorld;
				MainGame = mainGame;

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
				DisplaceContents();

				PhysicsObject.Destroy();
				Renderable.Destroy();

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

				GraphicsWorld.UnorderedRenderables.Remove(Renderable);

				foreach (var connector in Connectors)
				{
					connector.Disconnect(this);
				}
			}
		}

		protected virtual void DisplaceContents()
		{
		}

		void Entity.Tick()
		{
			Tick();
		}

		protected virtual void Tick()
		{
		}

		void Entity.Interacted()
		{
			Interacted();
		}

		protected virtual void Interacted()
		{ 
		}

		protected abstract void SerializeState(XmlWriter writer);

		void IConstruction.SerializeImpl(XmlWriter writer, Connector? sourceConnector, HashSet<IConstruction> serializedConstructions)
		{
			if (!serializedConstructions.Contains(this))
			{
				serializedConstructions.Add(this);
			}

			writer.WriteStartElement(nameof(Pipe));
			{
				writer.WriteElementString("PipeType", PipeType.Name);

				writer.WriteElementString("CustomTransformed", (Created || sourceConnector is null).ToString());

				if (!ConstructionInfo.HasValue || sourceConnector is null)
				{
					Transform.Serialize(writer);
				}
				else
				{
					ConstructionInfo.Value.Serialize(writer);
				}

				writer.WriteElementString("ConnectorCount", Connectors.Length.ToString());

				foreach (var connector in Connectors)
				{
					writer.WriteStartElement(nameof(Connector));
					{
						bool ignore = connector.Vacant || connector == sourceConnector || serializedConstructions.Contains((IConstruction)connector.GetOther(this));

						IConstruction construction = null;

						if (!ignore)
						{
							construction = (IConstruction)connector.GetOther(this);
						}

						writer.WriteElementString(nameof(ignore), ignore.ToString());

						if (!ignore)
						{
							serializedConstructions.Add(construction);

							construction.Serialize(writer, connector, serializedConstructions);
						}
					}
					writer.WriteEndElement();
				}

				writer.WriteStartElement("State");
				{
					SerializeState(writer);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		protected abstract void DeserializeState(XmlReader reader);

		static void IConstruction.DeserializeImpl(XmlReader reader, Connector? sourceConnector, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(Pipe));
			{
				PipeType pipeType = mainGame.PipeTypesDictionary[reader.ReadElementString("PipeType")];

				Pipe pipe;

				if (bool.Parse(reader.ReadElementString("CustomTransformed")))
				{
					Transform transform = reader.DeserializeTransform();

					pipe = Create(pipeType, transform, ui, physicsWorld, graphicsWorld, gameWorld, mainGame, referenceHandler);
				}
				else
				{
					ConstructionInfo constructionInfo = Space_Refinery_Game.ConstructionInfo.Deserialize(reader);

					pipe = (Pipe)Build(sourceConnector, pipeType, constructionInfo.IndexOfSelectedConnector, constructionInfo.Rotation, ui, physicsWorld, graphicsWorld, gameWorld, mainGame, referenceHandler);
				}

				int count = int.Parse(reader.ReadElementString("ConnectorCount"));

				for (int i = 0; i < count; i++)
				{
					reader.ReadStartElement(nameof(Connector));
					{
						bool ignore = bool.Parse(reader.ReadElementString(nameof(ignore)));

						if (!ignore)
						{
							IConstructionSerialization.Deserialize(reader, pipe.Connectors[i], ui, physicsWorld, graphicsWorld, gameWorld, mainGame, referenceHandler);
						}
					}
					reader.ReadEndElement();
				}

				reader.ReadStartElement("State");
				{
					pipe.DeserializeState(reader);
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}
	}
}
