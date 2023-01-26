using BepuPhysics.Collidables;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// CREDIT: ??, mellinoe

namespace Space_Refinery_Game
{
	public sealed class Starfield
	{
		public GraphicsWorld GraphicsWorld;

		public StarfieldRenderable Renderable;

		private Starfield(GraphicsWorld graphicsWorld, StarfieldRenderable renderable)
		{
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
		}

		public static Starfield Create(GraphicsWorld graphWorld)
		{
			StarfieldRenderable renderable = StarfieldRenderable.Create(graphWorld.ViewInfoBuffer, graphWorld.GraphicsDevice, graphWorld.Factory);

			graphWorld.AddRenderable(renderable, -1);

			return new(graphWorld, renderable);
		}
	}
}
