using Space_Refinery_Engine.Renderer;

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

	public static Sky CreateAndAdd(GraphicsWorld graphicsWorld)
	{
		SkyRenderable renderable = SkyRenderable.Create(graphicsWorld);

		var starfield = new Sky(graphicsWorld, renderable);

		starfield.AddToGraphicsWorld();

		return starfield;
	}

	public void AddToGraphicsWorld()
	{
		GraphicsWorld.AddRenderable(Renderable, -1);
	}
}
