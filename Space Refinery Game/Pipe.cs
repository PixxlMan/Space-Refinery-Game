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
	public class Pipe : Entity, IConstruction, IConnectable
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public Transform Transform { get; set; }

		public GraphicsWorld GraphicsWorld;

		public GameWorld GameWorld;

		public EntityRenderable Renderable;
		
		public PipeConnector[] Connectors;

		private IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		public PipeType PipeType;

		public ResourceContainer ResourceContainer = new();

		private Pipe(Transform transform)
		{
			informationProvider = new PipeInformationProvider(this);

			Transform = transform;
		}

		public void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanSetting>($"{nameof(Pipe)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static Pipe Create(PipeType pipeType, Transform transform, PhysicsWorld physWorld, GraphicsWorld graphWorld, GameWorld gameWorld)
		{
			Pipe pipe = new(transform);

			MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

			EntityRenderable renderable = CreateRenderable(pipeType, graphWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physWorld, transform, pipe);

			PipeConnector[] connectors = CreateConnectors(pipeType, pipe, physWorld, gameWorld);

			pipe.SetUp(pipeType, physWorld, physObj, connectors, graphWorld, renderable, gameWorld);

			gameWorld.AddEntity(pipe);

			return pipe;
		}

		private static EntityRenderable CreateRenderable(PipeType pipeType, GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, pipeType.Mesh, Utils.GetSolidColoredTexture(RgbaByte.LightGrey, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);
			return renderable;
		}

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, Pipe pipeStraight)
		{
			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(1, .5f, .5f), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);
			return physObj;
		}

		private static PipeConnector[] CreateConnectors(PipeType pipeType, Pipe pipe, PhysicsWorld physWorld, GameWorld gameWorld)
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
					PipeConnector connector = new PipeConnector(pipe, ConnectorSide.A, pipeType.ConnectorProperties[i], gameWorld);

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

					connector.Transform  = transform;

					var physicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(.4f, .4f, .25f), transform, 0, true);

					connector.PhysicsObject = physWorld.AddPhysicsObject(physicsObjectDescription, connector);

					ConnectorProxy connectorProxy = new(connector);

					var userInteractableObjectDescription = new PhysicsObjectDescription<Box>(new Box(.75f, .75f, .75f), transform, 0, true);

					connectorProxy.PhysicsObject = physWorld.AddPhysicsObject(userInteractableObjectDescription, connector);

					connector.Proxy = connectorProxy;

					connectors[i] = connector;

					gameWorld.AddEntity(connector);

					continue;
				}
				else if (physicsObject.Entity is PipeConnector pipeConnector)
				{
					pipeConnector.Connect(pipe);

					connectors[i] = pipeConnector;

					continue;
				}
			}

			return connectors;
		}

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld)
		{
			PipeConnector pipeConnector = (PipeConnector)connector;

			PipeType pipeType = (PipeType)entityType;

			Transform transform = GameWorld.GenerateTransformForConnector(pipeType.ConnectorPlacements[indexOfSelectedConnector], pipeConnector, rotation);

			Pipe pipe = new(transform);

			MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

			EntityRenderable renderable = CreateRenderable(pipeType, graphicsWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physicsWorld, transform, pipe);

			var connectors = CreateConnectors(pipeType, pipe, physicsWorld, gameWorld);

			pipe.SetUp(pipeType, physicsWorld, physObj, connectors, graphicsWorld, renderable, gameWorld);

			gameWorld.AddEntity(pipe);

			return pipe;
		}

		private void SetUp(PipeType pipeType, PhysicsWorld physicsWorld, PhysicsObject physicsObject, PipeConnector[] connectors, GraphicsWorld graphicsWorld, EntityRenderable renderable, GameWorld gameWorld)
		{
			PipeType = pipeType;
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			Connectors = connectors;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
			GameWorld = gameWorld;
		}

		public void Deconstruct()
		{
			PhysicsObject.Destroy();
			Renderable.Destroy();

			MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

			GraphicsWorld.UnorderedRenderables.Remove(Renderable);

			foreach (var connector in Connectors)
			{
				connector.Disconnect(this);
			}
		}

		public FixedDecimalLong8 Fullness => ResourceContainer.GetVolume() / (FixedDecimalLong8)PipeType.PipeProperties.FlowableVolume;

		void Entity.Tick()
		{

		}
	}
}
