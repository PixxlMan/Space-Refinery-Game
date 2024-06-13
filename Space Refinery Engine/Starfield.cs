using Space_Refinery_Game_Renderer;
using Space_Refinery_Game_Renderer;

// CREDIT: ??, mellinoe

namespace Space_Refinery_Engine;

public sealed class Starfield
{
	public GraphicsWorld GraphicsWorld { get; }

	public StarfieldRenderable Renderable { get; }

	private Starfield(GraphicsWorld graphicsWorld, StarfieldRenderable renderable)
	{
		GraphicsWorld = graphicsWorld;
		Renderable = renderable;
	}

	public static Starfield CreateAndAdd(GraphicsWorld graphWorld)
	{
		StarfieldRenderable renderable = StarfieldRenderable.Create(graphWorld.ViewInfoBuffer, graphWorld.GraphicsDevice, graphWorld.Factory);

		var starfield = new Starfield(graphWorld, renderable);

		starfield.AddToGraphicsWorld();

		return starfield;
	}

	public void AddToGraphicsWorld()
	{
		GraphicsWorld.AddRenderable(Renderable, -1);
	}
}
