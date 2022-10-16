using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class GameData
	{
		private UI uI;
		private PhysicsWorld physicsWorld;
		private GraphicsWorld graphicsWorld;
		private GameWorld gameWorld;
		private MainGame mainGame;
		private SerializationReferenceHandler referenceHandler;

		public GameData()
		{

		}

		public GameData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			UI = ui;
			PhysicsWorld = physicsWorld;
			GraphicsWorld = graphicsWorld;
			GameWorld = gameWorld;
			MainGame = mainGame;
			ReferenceHandler = referenceHandler;

			UpdatePerformanceStatisticsCollector();
		}

		public PerformanceStatisticsCollector PerformanceStatisticsCollector { get; private set; }

		void UpdatePerformanceStatisticsCollector()
		{
			PerformanceStatisticsCollector = new(this);
		}

		public UI UI { get => uI; set { uI = value; UpdatePerformanceStatisticsCollector(); } }

		public PhysicsWorld PhysicsWorld { get => physicsWorld; set { physicsWorld = value; UpdatePerformanceStatisticsCollector(); } }

		public GraphicsWorld GraphicsWorld { get => graphicsWorld; set { graphicsWorld = value; UpdatePerformanceStatisticsCollector(); } }

		public GameWorld GameWorld { get => gameWorld; set { gameWorld = value; UpdatePerformanceStatisticsCollector(); } }

		public MainGame MainGame { get => mainGame; set { mainGame = value; UpdatePerformanceStatisticsCollector(); } }

		public SerializationReferenceHandler ReferenceHandler { get => referenceHandler; set { referenceHandler = value; UpdatePerformanceStatisticsCollector(); } }
	}
}
