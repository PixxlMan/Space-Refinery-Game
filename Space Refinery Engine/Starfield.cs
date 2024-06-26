﻿using Space_Refinery_Engine.Renderer;

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

	public static Starfield CreateAndAdd(GraphicsWorld graphicsWorld)
	{
		StarfieldRenderable renderable = StarfieldRenderable.Create(graphicsWorld);

		var starfield = new Starfield(graphicsWorld, renderable);

		starfield.AddToGraphicsWorld();

		return starfield;
	}

	public void AddToGraphicsWorld()
	{
		GraphicsWorld.AddRenderable(Renderable, -1);
	}
}
