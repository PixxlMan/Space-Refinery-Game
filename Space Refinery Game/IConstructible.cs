﻿using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public interface IConstructible
	{
		public string TargetName { get; }

		public Func<Connector, PhysicsWorld, GraphicsWorld, IConstruction> Build { get; }
	}
}