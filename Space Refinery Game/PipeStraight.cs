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

		public PhysicsObject[] PhysicsObjectConnectors;

		public GraphicsWorld GraphicsWorld;

		public EntityRenderable Renderable;

		private IInformationProvider informationProvider;

		public override IInformationProvider InformationProvider => informationProvider;

		private PipeStraight()
		{
			informationProvider = new PipeStraightInformationProvider(this);
		}

		public static PipeStraight Create(PhysicsWorld physWorld, GraphicsWorld graphWorld, Transform transform)
		{
			PipeStraight pipeStraight = new();

			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, FXRenderer.Mesh.LoadMesh(graphWorld.GraphicsDevice, graphWorld.Factory, Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe"), "PipeStraight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Green, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);

			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(1, .5f, .5f), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);

			Transform connectorA = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + ((ITransformable)transform).LocalUnitX * 0.5f };

			Transform connectorB = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + -((ITransformable)transform).LocalUnitX * 0.5f };

			MainGame.DebugRender.DrawCube(connectorA, RgbaFloat.Blue);

			MainGame.DebugRender.DrawCube(connectorB, RgbaFloat.Cyan);

			PipeConnector pipeConnectorA = new();

			PipeConnector pipeConnectorB = new();

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

			pipeStraight.SetUp(physWorld, physObj, connectorPhysicsObjects, graphWorld, renderable);

			return pipeStraight;
		}

		private void SetUp(PhysicsWorld physicsWorld, PhysicsObject physicsObject, PhysicsObject[] physicsObjectConnectors, GraphicsWorld graphicsWorld, EntityRenderable renderable)
		{
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			PhysicsObjectConnectors = physicsObjectConnectors;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
		}
	}
}
