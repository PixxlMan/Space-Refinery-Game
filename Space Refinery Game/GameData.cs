using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class GameData
	{
		public enum GameDataChange
		{
			UI,
			PhysicsWorld,
			GraphicsWorld,
			GameWorld,
			MainGame,
			ReferenceHandler,
		}

		private UI uI;
		private PhysicsWorld physicsWorld;
		private GraphicsWorld graphicsWorld;
		private GameWorld gameWorld;
		private MainGame mainGame;
		private SerializationReferenceHandler referenceHandler;

		public event Action<GameDataChange> GameDataChangedEvent;

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

			PerformanceStatisticsCollector = new(this, PerformanceStatisticsCollector.PerformanceStatisticsCollectorMode.Averaged);
		}

		public PerformanceStatisticsCollector PerformanceStatisticsCollector { get; private set; }

		void GameDataChanged(GameDataChange gameDataChange)
		{
			if (PerformanceStatisticsCollector is null)
			{
				PerformanceStatisticsCollector = new(this, PerformanceStatisticsCollector.PerformanceStatisticsCollectorMode.Averaged);
			}

			GameDataChangedEvent?.Invoke(gameDataChange);
		}

		public UI UI { get => uI; set { uI = value; GameDataChanged(GameDataChange.UI); } }

		public PhysicsWorld PhysicsWorld { get => physicsWorld; set { physicsWorld = value; GameDataChanged(GameDataChange.PhysicsWorld); } }

		public GraphicsWorld GraphicsWorld { get => graphicsWorld; set { graphicsWorld = value; GameDataChanged(GameDataChange.GraphicsWorld); } }

		public GameWorld GameWorld { get => gameWorld; set { gameWorld = value; GameDataChanged(GameDataChange.GameWorld); } }

		public MainGame MainGame { get => mainGame; set { mainGame = value; GameDataChanged(GameDataChange.MainGame); } }

		public SerializationReferenceHandler ReferenceHandler { get => referenceHandler; set { referenceHandler = value; GameDataChanged(GameDataChange.ReferenceHandler); } }
	}
}
