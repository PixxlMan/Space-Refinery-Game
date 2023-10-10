using Space_Refinery_Game_Renderer;
using Space_Refinery_Engine.Audio;

namespace Space_Refinery_Engine
{
	public sealed record class GameData
	{
		public enum GameDataChange
		{
			UI,
			PhysicsWorld,
			GraphicsWorld,
			AudioWorld,
			GameWorld,
			MainGame,
			ReferenceHandler,
			Settings,
		}

		private UI uI;
		private PhysicsWorld physicsWorld;
		private GraphicsWorld graphicsWorld;
		private AudioWorld audioWorld;
		private GameWorld gameWorld;
		private MainGame mainGame;
		private SerializationReferenceHandler referenceHandler;
		private Settings settings;

		public event Action<GameDataChange> GameDataChangedEvent;

		public GameData()
		{

		}

		public GameData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, AudioWorld audioWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler, Settings settings)
		{
			UI = ui;
			PhysicsWorld = physicsWorld;
			GraphicsWorld = graphicsWorld;
			AudioWorld = audioWorld;
			GameWorld = gameWorld;
			MainGame = mainGame;
			ReferenceHandler = referenceHandler;
			Settings = settings;

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

		public AudioWorld AudioWorld { get => audioWorld; set { audioWorld = value; GameDataChanged(GameDataChange.AudioWorld); } }

		public GameWorld GameWorld { get => gameWorld; set { gameWorld = value; GameDataChanged(GameDataChange.GameWorld); } }

		public MainGame MainGame { get => mainGame; set { mainGame = value; GameDataChanged(GameDataChange.MainGame); } }

		public SerializationReferenceHandler ReferenceHandler { get => referenceHandler; set { referenceHandler = value; GameDataChanged(GameDataChange.ReferenceHandler); } }

		public Settings Settings { get => settings; set { settings = value; GameDataChanged(GameDataChange.Settings); } }
	}
}
