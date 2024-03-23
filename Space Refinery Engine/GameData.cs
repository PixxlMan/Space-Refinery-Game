using Space_Refinery_Game_Renderer;
using Space_Refinery_Engine.Audio;

namespace Space_Refinery_Engine
{
	public sealed record class GameData
	{
		public enum GameDataChange
		{
			GraphicsWorld,
			PhysicsWorld,
			AudioWorld,
			MainGame,
			Settings,
			Game,
			UI,
		}

		private GraphicsWorld graphicsWorld;
		private PhysicsWorld physicsWorld;
		private AudioWorld audioWorld;
		private MainGame mainGame;
		private Settings settings;
		private Game game;
		private UI uI;

		public WeakEvent<GameDataChange> GameDataChangedEvent;

		public GameData()
		{

		}

		public GameData(GraphicsWorld graphicsWorld, PhysicsWorld physicsWorld, AudioWorld audioWorld, MainGame mainGame, Settings settings, Game game, UI uI)
		{
			this.graphicsWorld = graphicsWorld;
			this.physicsWorld = physicsWorld;
			this.audioWorld = audioWorld;
			this.mainGame = mainGame;
			this.settings = settings;
			this.game = game;
			this.uI = uI;

			PerformanceStatisticsCollector = new(this, PerformanceStatisticsCollector.PerformanceStatisticsCollectorMode.Averaged);
		}

		public PerformanceStatisticsCollector PerformanceStatisticsCollector { get; private set; }

		private void GameDataChanged(GameDataChange gameDataChange)
		{
			if (PerformanceStatisticsCollector is null)
			{
				PerformanceStatisticsCollector = new(this, PerformanceStatisticsCollector.PerformanceStatisticsCollectorMode.Averaged);
			}

			GameDataChangedEvent?.Invoke(gameDataChange);
		}

		public void Reset()
		{
			GameDataChangedEvent = null;
		}

		public void Restore()
		{
			PerformanceStatisticsCollector.Restore();
		}

		public GraphicsWorld GraphicsWorld { get => graphicsWorld; set { graphicsWorld = value; GameDataChanged(GameDataChange.GraphicsWorld); } }

		public PhysicsWorld PhysicsWorld { get => physicsWorld; set { physicsWorld = value; GameDataChanged(GameDataChange.PhysicsWorld); } }

		public AudioWorld AudioWorld { get => audioWorld; set { audioWorld = value; GameDataChanged(GameDataChange.AudioWorld); } }

		public MainGame MainGame { get => mainGame; set { mainGame = value; GameDataChanged(GameDataChange.MainGame); } }

		public Settings Settings { get => settings; set { settings = value; GameDataChanged(GameDataChange.Settings); } }

		public Game Game { get => game; set { game = value; GameDataChanged(GameDataChange.Game); } }

		public UI UI { get => uI; set { uI = value; GameDataChanged(GameDataChange.UI); } }
	}
}
