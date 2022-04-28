﻿using FixedPrecision;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public interface IConstruction//<TConnector> where TConnector : Connector
	{
		public static abstract IConstruction/*<TConnector>*/ Build(/*TConnector*/ Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame);

		public void Deconstruct();

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}