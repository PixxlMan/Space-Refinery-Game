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
	public class PipeStraight : Pipe
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public (PipeConnector connectorA, PipeConnector connectorB) Connectors;

		public GraphicsWorld GraphicsWorld;

		public EntityRenderable Renderable;

		public Transform Transform { get; set; }

		private IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		private PipeStraight(Transform transform)
		{
			informationProvider = new PipeStraightInformationProvider(this);

			Transform = transform;
		}

		public static PipeStraight Create(PhysicsWorld physWorld, GraphicsWorld graphWorld, Transform transform)
		{
			PipeStraight pipeStraight = new(transform);

			EntityRenderable renderable = CreateRenderable(graphWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physWorld, transform, pipeStraight);

			var connectors = CreateConnectors(pipeStraight, physWorld, transform);

			pipeStraight.SetUp(physWorld, physObj, connectors, graphWorld, renderable);

			return pipeStraight;
		}

		private static EntityRenderable CreateRenderable(GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, FXRenderer.Mesh.LoadMesh(graphWorld.GraphicsDevice, graphWorld.Factory, Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe"), "PipeStraight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Green, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);
			return renderable;
		}

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, PipeStraight pipeStraight)
		{
			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(1, .5f, .5f), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);
			return physObj;
		}

		private static (PipeConnector connectorA, PipeConnector connectorB) CreateConnectors(PipeStraight pipeStraight, PhysicsWorld physWorld, Transform transform)
		{
			Transform connectorA = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + ((ITransformable)transform).LocalUnitX * 0.5f };

			Transform connectorB = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + -((ITransformable)transform).LocalUnitX * 0.5f };

			MainGame.DebugRender.DrawCube(connectorA, RgbaFloat.Blue);

			MainGame.DebugRender.DrawCube(connectorB, RgbaFloat.Cyan);

			PipeConnector pipeConnectorA = new(pipeStraight, ConnectorSide.A);

			PipeConnector pipeConnectorB = new(pipeStraight, ConnectorSide.B);

			PhysicsObjectDescription<Box>[] physicsObjectConnectorDescription = new PhysicsObjectDescription<Box>[]
			{
				new(new Box(connectorA.Scale.X.ToFloat(), connectorA.Scale.Y.ToFloat(), connectorA.Scale.Z.ToFloat()), connectorA, 0, true),
				new(new Box(connectorB.Scale.X.ToFloat(), connectorB.Scale.Y.ToFloat(), connectorB.Scale.Z.ToFloat()), connectorB, 0, true),
			};

			PhysicsObject[] connectorPhysicsObjects = new PhysicsObject[]
			{
				physWorld.AddPhysicsObject(physicsObjectConnectorDescription[0], pipeConnectorA),
				physWorld.AddPhysicsObject(physicsObjectConnectorDescription[1], pipeConnectorB),
			};

			pipeConnectorA.PhysicsObject = connectorPhysicsObjects[0];
			pipeConnectorB.PhysicsObject = connectorPhysicsObjects[1];

			return (pipeConnectorA, pipeConnectorB);
		}

		private static (PipeConnector connectorA, PipeConnector connectorB) CreateConnectors(PipeStraight pipeStraight, PipeConnector existingConnector, PhysicsWorld physWorld, Transform transform)
		{
			Vector3FixedDecimalInt4 connectorPositionOffset = ((ITransformable)transform).LocalUnitX / 2;
			Transform otherConnectorTransform = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + (existingConnector.VacantSide == ConnectorSide.A ? -connectorPositionOffset : connectorPositionOffset) };

			MainGame.DebugRender.DrawCube(otherConnectorTransform, RgbaFloat.Blue);

			PipeConnector otherPipeConnector = new(pipeStraight, existingConnector.PopulatedSide.Value);

			var otherConnectorPhysicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(otherConnectorTransform.Scale.X.ToFloat(), otherConnectorTransform.Scale.Y.ToFloat(), otherConnectorTransform.Scale.Z.ToFloat()), otherConnectorTransform, 0, true);

			PhysicsObject otherConnectorPhysicsObject = physWorld.AddPhysicsObject(otherConnectorPhysicsObjectDescription, otherPipeConnector);

			otherPipeConnector.PhysicsObject = otherConnectorPhysicsObject;

			if (existingConnector.VacantSide == ConnectorSide.A)
			{
				existingConnector.Connect(pipeStraight);

				return (otherPipeConnector, existingConnector);
			}
			else // Vacant side is B.
			{
				existingConnector.Connect(pipeStraight);

				return (existingConnector, otherPipeConnector);
			}
		}

		public static IConstruction Build(Connector connector, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld)
		{
			PipeConnector pipeConnector = (PipeConnector)connector;

			Vector3FixedDecimalInt4 position = pipeConnector.UnconnectedPipe.Transform.Position + (pipeConnector.VacantSide == ConnectorSide.A ? ((ITransformable)pipeConnector.UnconnectedPipe.Transform).LocalUnitX : -((ITransformable)pipeConnector.UnconnectedPipe.Transform).LocalUnitX);

			Transform transform = new()
			{
				Position = position,
				Rotation = pipeConnector.UnconnectedPipe.Transform.Rotation,
				Scale = pipeConnector.UnconnectedPipe.Transform.Scale,
			};

			PipeStraight pipeStraight = new(transform);

			EntityRenderable renderable = CreateRenderable(graphicsWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physicsWorld, transform, pipeStraight);

			var connectors = CreateConnectors(pipeStraight, pipeConnector, physicsWorld, transform);

			pipeStraight.SetUp(physicsWorld, physObj, connectors, graphicsWorld, renderable);

			return pipeStraight;
		}
		
		private void SetUp(PhysicsWorld physicsWorld, PhysicsObject physicsObject, (PipeConnector connectorA, PipeConnector connectorB) connectors, GraphicsWorld graphicsWorld, EntityRenderable renderable)
		{
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			Connectors = connectors;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
		}
	}
}
