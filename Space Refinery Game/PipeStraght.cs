using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	public class PipeStraght
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public GraphicsWorld GraphicsWorld;

		public EntityRenderable Renderable;

		private PipeStraght(PhysicsWorld physicsWorld, PhysicsObject physicsObject, GraphicsWorld graphicsWorld, EntityRenderable renderable)
		{
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
		}

		public static PipeStraght Create(PhysicsWorld physWorld, GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, FXRenderer.Mesh.LoadMesh(graphWorld.GraphicsDevice, graphWorld.Factory, Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe"), "PipeStraight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Green, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);

			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(.5f, .5f, 1), transform);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription);

			return new(physWorld, physObj, graphWorld, renderable);
		}
	}
}
