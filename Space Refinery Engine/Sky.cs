using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Engine;

public sealed class Sky
{
	public GraphicsWorld GraphicsWorld { get; }

	public SkyRenderable Renderable { get; }

	private Sky(GraphicsWorld graphicsWorld, SkyRenderable renderable)
	{
		GraphicsWorld = graphicsWorld;
		Renderable = renderable;
	}

	public static Sky CreateAndAdd(GraphicsWorld graphWorld)
	{
		SkyRenderable renderable = SkyRenderable.Create(graphWorld.ViewInfoBuffer, graphWorld.GraphicsDevice, graphWorld.Factory);

		var starfield = new Sky(graphWorld, renderable);

		starfield.AddToGraphicsWorld();

		return starfield;
	}

	public void AddToGraphicsWorld()
	{
		GraphicsWorld.AddRenderable(Renderable, -1);
	}
}
