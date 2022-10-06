using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class GameData
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

		public UI UI { get; set; }

		public PhysicsWorld PhysicsWorld { get; set; }

		public GraphicsWorld GraphicsWorld { get; set; }

		public GameWorld GameWorld { get; set; }

		public MainGame MainGame { get; set; }

		public SerializationReferenceHandler ReferenceHandler { get; set; }
	}
}
