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
	public class PipeStraght
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public PhysicsObject[] PhysicsObjectConnectors;

		public GraphicsWorld GraphicsWorld;

		public EntityRenderable Renderable;

		public IInformationProvider InformationProvider;

		private PipeStraght(PhysicsWorld physicsWorld, PhysicsObject physicsObject, PhysicsObject[] physicsObjectConnectors, GraphicsWorld graphicsWorld, EntityRenderable renderable, IInformationProvider informationProvider)
		{
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			PhysicsObjectConnectors = physicsObjectConnectors;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
			InformationProvider = informationProvider;
		}

		public static PipeStraght Create(PhysicsWorld physWorld, GraphicsWorld graphWorld, Transform transform)
		{
			IInformationProvider informationProvider = new PipeStraightInformationProvider(null);

			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, FXRenderer.Mesh.LoadMesh(graphWorld.GraphicsDevice, graphWorld.Factory, Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe"), "PipeStraight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Green, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);

			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(1, .5f, .5f), transform, 0, true, informationProvider);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription);

			Transform connectorA = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + ((ITransformable)transform).LocalUnitX * 0.5f };

			Transform connectorB = new Transform(default, transform.Rotation, new(.25f, .5f, .5f)) { Position = transform.Position + -((ITransformable)transform).LocalUnitX * 0.5f };

			MainGame.DebugRender.DrawCube(connectorA, RgbaFloat.Blue);

			MainGame.DebugRender.DrawCube(connectorB, RgbaFloat.Cyan);

			ConnectorInformationProvider connectorInformationProviderConnectorA = new();

			ConnectorInformationProvider connectorInformationProviderConnectorB = new();

			PhysicsObjectDescription<Box>[] physicsObjectConnectorDescription = new PhysicsObjectDescription<Box>[]
			{
				new(new Box(connectorA.Scale.X.ToFloat(), connectorA.Scale.Y.ToFloat(), connectorA.Scale.Z.ToFloat()), connectorA, 0, true, connectorInformationProviderConnectorA),
				new(new Box(connectorB.Scale.X.ToFloat(), connectorB.Scale.Y.ToFloat(), connectorB.Scale.Z.ToFloat()), connectorB, 0, true, connectorInformationProviderConnectorB),
			};

			PhysicsObject[] physicsObjects = new PhysicsObject[]
			{
				physWorld.AddPhysicsObject(physicsObjectConnectorDescription[0]),
				physWorld.AddPhysicsObject(physicsObjectConnectorDescription[1]),
			};

			PipeStraght pipeStraght = new(physWorld, physObj, physicsObjects, graphWorld, renderable, informationProvider);

			((PipeStraightInformationProvider)informationProvider).PipeStraght = pipeStraght;

			return pipeStraght;
		}
	}
}
