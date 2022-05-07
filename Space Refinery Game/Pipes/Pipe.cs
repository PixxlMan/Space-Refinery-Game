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

		protected Dictionary<string, PipeConnector> NamedConnectors = new();

		public abstract void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, PipeConnector transferingConnector);

		public virtual void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Pipe)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static Pipe Create(PipeType pipeType, Transform transform, UI ui, PhysicsWorld physWorld, GraphicsWorld graphWorld, GameWorld gameWorld, MainGame mainGame)
		{
			Pipe pipe = (Pipe)Activator.CreateInstance(pipeType.TypeOfPipe, true);

			pipe.Transform = transform;

			MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

			EntityRenderable renderable = CreateRenderable(pipeType, graphWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physWorld, transform, pipe, pipeType.Mesh);

			PipeConnector[] connectors = CreateConnectors(pipeType, pipe, physWorld, gameWorld, ui);

			pipe.SetUp(pipeType, ui, physWorld, physObj, connectors, graphWorld, renderable, gameWorld, mainGame);

			gameWorld.AddEntity(pipe);

			return pipe;
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
				PhysicsObject physicsObject = physWorld.Raycast<PipeConnector>(
					pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation) * (FixedDecimalInt4)1.25f,
					-Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Direction, pipe.Transform.Rotation),
					(FixedDecimalInt4).125f);

				MainGame.DebugRender.PersistentRay(
					pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation) * (FixedDecimalInt4)1.25f,
					-Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Direction, pipe.Transform.Rotation),
					RgbaFloat.Yellow);
				
				if (physicsObject is null || physicsObject.Entity is not PipeConnector)
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
				else if (physicsObject.Entity is PipeConnector pipeConnector)
				{
					pipeConnector.Connect(pipe);

					connectors[i] = pipeConnector;

					if (pipeType.ConnectorNames is not null && pipeType.ConnectorNames[i] is not null)
					{
						pipe.NamedConnectors.Add(pipeType.ConnectorNames[i], pipeConnector);
					}

					continue;
				}
			}

			return connectors;
		}

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame)
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

			pipe.SetUp(pipeType, ui, physicsWorld, physObj, connectors, graphicsWorld, renderable, gameWorld, mainGame);

			gameWorld.AddEntity(pipe);

			return pipe;
		}

		private void SetUp(PipeType pipeType, UI ui, PhysicsWorld physicsWorld, PhysicsObject physicsObject, PipeConnector[] connectors, GraphicsWorld graphicsWorld, EntityRenderable renderable, GameWorld gameWorld, MainGame mainGame)
		{
			lock (this)
			{
				PipeType = pipeType;
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
	}
}
