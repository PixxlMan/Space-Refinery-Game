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
	public class Pipe : Entity, IConstruction
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public Transform Transform { get; set; }

		public GraphicsWorld GraphicsWorld;

		public EntityRenderable Renderable;

		public PipeConnector[] Connectors;

		private IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		public PipeType PipeType;

		private Pipe(Transform transform)
		{
			informationProvider = new PipeStraightInformationProvider(this);

			Transform = transform;
		}

		public static Pipe Create(PipeType pipeType, Transform transform, PhysicsWorld physWorld, GraphicsWorld graphWorld)
		{
			Pipe pipeStraight = new(transform);

			EntityRenderable renderable = CreateRenderable(graphWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physWorld, transform, pipeStraight);

			PipeConnector[] connectors = CreateConnectors(pipeType, pipeStraight, physWorld);

			pipeStraight.SetUp(physWorld, physObj, connectors, graphWorld, renderable);

			return pipeStraight;
		}

		private static EntityRenderable CreateRenderable(GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, FXRenderer.Mesh.LoadMesh(graphWorld.GraphicsDevice, graphWorld.Factory, Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe"), "PipeStraight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Green, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);
			return renderable;
		}

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, Pipe pipeStraight)
		{
			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(1, .5f, .5f), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);
			return physObj;
		}

		private static PipeConnector[] CreateConnectors(PipeType pipeType, Pipe pipe, PhysicsWorld physWorld)
		{
			PipeConnector[] connectors = new PipeConnector[pipeType.ConnectorPlacements.Length];

			for (int i = 0; i < pipeType.ConnectorPlacements.Length; i++)
			{
				var physicsObject = physWorld.Raycast(pipeType.ConnectorPlacements[i].Position + pipe.Transform.Position, pipeType.ConnectorPlacements[i].Direction, 0.5f);

				if (physicsObject is null || physicsObject.Entity is not PipeConnector)
				{
					PipeConnector connector = new PipeConnector(pipe, ConnectorSide.A);

					Transform transform = new(pipe.Transform.Position + pipeType.ConnectorPlacements[i].Position, QuaternionFixedDecimalInt4.CreateLookingAt(Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Direction, pipe.Transform.Rotation)));

					connector.Transform = transform;

					var physicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(.4f, .4f, .25f), transform, 0, true);

					connector.PhysicsObject = physWorld.AddPhysicsObject(physicsObjectDescription, connector);

					connectors[i] = connector;

					MainGame.DebugRender.DrawCube(new(transform) { Scale = new(.4f, .4f, .25f) }, RgbaFloat.CornflowerBlue);

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

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld)
		{
			PipeConnector pipeConnector = (PipeConnector)connector;

			PipeType pipeType = (PipeType)entityType;

			Transform transform = new(pipeConnector.Transform.Position + -pipeType.ConnectorPlacements[indexOfSelectedConnector].Position, QuaternionFixedDecimalInt4.CreateLookingAt(Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[indexOfSelectedConnector].Direction, pipeConnector.Transform.Rotation)));

			Pipe pipeStraight = new(transform);

			EntityRenderable renderable = CreateRenderable(graphicsWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physicsWorld, transform, pipeStraight);

			var connectors = CreateConnectors(pipeType, pipeStraight, physicsWorld);

			pipeStraight.SetUp(physicsWorld, physObj, connectors, graphicsWorld, renderable);

			return pipeStraight;
		}
		
		private void SetUp(PhysicsWorld physicsWorld, PhysicsObject physicsObject, PipeConnector[] connectors, GraphicsWorld graphicsWorld, EntityRenderable renderable)
		{
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			Connectors = connectors;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
		}

		public void Deconstruct()
		{
			PhysicsObject.Destroy();
			Renderable.Destroy();

			GraphicsWorld.UnorderedRenderables.Remove(Renderable);

			foreach (var connector in Connectors)
			{
				connector.Disconnect(this);
			}
		}
	}
}
