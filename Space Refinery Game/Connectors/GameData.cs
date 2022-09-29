using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public class GameData
	{
		public GameData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			UI = ui;
			PhysicsWorld = physicsWorld;
			GraphicsWorld = graphicsWorld;
			GameWorld = gameWorld;
			MainGame = mainGame;
			ReferenceHandler = referenceHandler;
		}

		public UI UI { get; }

		public PhysicsWorld PhysicsWorld { get; }

		public GraphicsWorld GraphicsWorld { get; }

		public GameWorld GameWorld { get; }

		public MainGame MainGame { get; }

		public SerializationReferenceHandler ReferenceHandler { get; }
	}
}
