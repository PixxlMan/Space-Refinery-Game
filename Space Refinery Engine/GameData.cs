using Space_Refinery_Game.Renderer;
using Space_Refinery_Engine.Audio;
using System.Diagnostics;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed record class GameData
{
	public enum GameDataChange
	{
		GraphicsWorld,
		PhysicsWorld,
		InputUpdate,
		AudioWorld,
		Settings,
		Game,
		UI,
	}

	public static DebugRender DebugRender { get; internal set; }
	public static DebugSettings DebugSettings { get; internal set; } = new();

	public static SerializationReferenceHandler GlobalReferenceHandler { get; internal set; }

	private GraphicsWorld graphicsWorld;
	private PhysicsWorld physicsWorld;
	private InputUpdate inputUpdate;
	private AudioWorld audioWorld;
	private Settings settings;
	private Game game;
	private UI uI;

	public Extension EngineExtension { get; internal set; }
	public ICollection<Extension> Extensions { get; internal set; }

	private const long TRUE = long.MaxValue;
	private const long FALSE = 0;

	private long paused; // TODO: move to GameData
	public bool Paused
	{
		get
		{
			return Interlocked.Read(ref paused) == TRUE;
		}
		private set
		{
			Interlocked.Exchange(ref paused, value ? TRUE : FALSE);
		}
	}

	public event Action<GameDataChange> GameDataChangedEvent;

	public GameData()
	{

	}

	public GameData(GraphicsWorld graphicsWorld, PhysicsWorld physicsWorld, InputUpdate inputUpdate, AudioWorld audioWorld, Settings settings, Game game, UI uI)
	{
		this.graphicsWorld = graphicsWorld;
		this.physicsWorld = physicsWorld;
		this.inputUpdate = inputUpdate;
		this.audioWorld = audioWorld;
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

	public void ChangePauseState(bool paused)
	{
		InputTracker.IgnoreNextFrameMousePosition = true;

		Paused = paused;

		GraphicsWorld.Window.CaptureMouse = !paused;
	}

	public void Serialize(string path)
	{
		lock (Game.GameWorld.TickSyncObject)
		{
			Logging.LogScopeStart($"Serializing {Game.GameReferenceHandler.ReferenceCount} references");

			File.Delete(path);

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using FileStream stream = File.OpenWrite(path);

			using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });

			writer.WriteStartDocument();
			{
				writer.SerializeWithoutEmbeddedType(Game, nameof(Game));
			}
			writer.WriteEndDocument();

			writer.Flush();
			writer.Close();
			stream.Flush(true);
			stream.Close();
			writer.Dispose();
			stream.Dispose();

			stopwatch.Stop();

			Logging.Log($"Serialized all state in {stopwatch.Elapsed.TotalMilliseconds} ms");

			Logging.LogScopeEnd();
		}
	}

	public void Deserialize(string path)
	{
		lock (Game.GameWorld.TickSyncObject)
		{
			Logging.LogScopeStart($"Deserializing");

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using Stream stream = File.OpenRead(path);

			using XmlReader reader = XmlReader.Create(stream);

			var serializationData = new SerializationData(this);

			//Game?.Destroy();

			Game?.GameWorld.Destroy();

			Game = reader.DeserializeEntitySerializableWithoutEmbeddedType<Game>(serializationData, null, nameof(Game));

			serializationData.SerializationComplete();

			reader.Close();

			stream.Close();

			reader.Dispose();

			stream.Dispose();

			stopwatch.Stop();

			Logging.Log($"Deserialized all {Game.GameReferenceHandler.ReferenceCount} references in {stopwatch.Elapsed.TotalMilliseconds} ms");

			Logging.LogScopeEnd();
		}
	}

	public GraphicsWorld GraphicsWorld { get => graphicsWorld; set { graphicsWorld = value; GameDataChanged(GameDataChange.GraphicsWorld); } }

	public PhysicsWorld PhysicsWorld { get => physicsWorld; set { physicsWorld = value; GameDataChanged(GameDataChange.PhysicsWorld); } }

	public InputUpdate InputUpdate { get => inputUpdate; set { inputUpdate = value; GameDataChanged(GameDataChange.InputUpdate); } }

	public AudioWorld AudioWorld { get => audioWorld; set { audioWorld = value; GameDataChanged(GameDataChange.AudioWorld); } }

	public Settings Settings { get => settings; set { settings = value; GameDataChanged(GameDataChange.Settings); } }

	public Game Game { get => game; set { game = value; GameDataChanged(GameDataChange.Game); } }

	public UI UI { get => uI; set { uI = value; GameDataChanged(GameDataChange.UI); } }
}
