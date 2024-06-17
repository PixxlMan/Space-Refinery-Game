using Space_Refinery_Game_Renderer;
using Space_Refinery_Engine.Audio;
using System.Diagnostics;
using System.Xml;
using Veldrid;
using BepuPhysics;
using BepuUtilities.Memory;
using BepuUtilities;

namespace Space_Refinery_Engine;

public sealed class MainGame // TODO: make everything thread safe! or is it already, it's just that it cannot change at runtime anyways?
{
	public GameData GameData { get; private set; }

	private const long TRUE = long.MaxValue;
	private const long FALSE = 0;

	private long paused;
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

	public readonly object UpdateSyncObject = new();


	public event Action<IntervalUnit>? CollectUpdatePerformanceData;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.


	public static SerializationReferenceHandler GlobalReferenceHandler { get; internal set; }

	public static Extension EngineExtension { get; internal set; }

	public static ICollection<Extension> Extensions { get; internal set; }

	public static DebugRender DebugRender;

	public static DebugSettings DebugSettings = new();


	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		// TODO: explain initialization dependencies here!

		Logging.LogScopeStart("Game initialization");

		GameData = new()
		{
			MainGame = this
		};

		InputTracker.ListenToWindow(window);

		GameData.GraphicsWorld = new();
		GameData.GraphicsWorld.SetUp(window, gd, factory, swapchain);
		DebugRender = DebugRender.Create(GameData.GraphicsWorld);

		GlobalReferenceHandler = new();
		GlobalReferenceHandler.EnterAllowEventualReferenceMode(false);
		{

			GameData.Settings = new(GameData);

			GameData.AudioWorld = AudioWorld.Create(GameData);

			GameData.AudioWorld.MusicSystem.SetTags(MusicTag.Intense);

			DebugSettings.AccessSetting("Fill music queue", (ActionDebugSetting)GameData.AudioWorld.MusicSystem.FillQueue);

			ResourceDeserialization.DeserializeIntoGlobalReferenceHandler(GlobalReferenceHandler, GameData, out var extensions);
			Extensions = extensions;

			GameData.Settings.LoadSettingValuesFromSettingsFile();

			foreach (Extension extension in Extensions)
			{
				extension.ExtensionObject?.OnGlobalReferenceHandlerDeserialization(GlobalReferenceHandler, GameData);
			}

			Debug.Assert(GlobalReferenceHandler.AllowEventualReferences, $"{nameof(GlobalReferenceHandler.AllowEventualReferences)} mode was disabled at some point deserialization into {nameof(GlobalReferenceHandler)}. It should not be deactivated as that can cause issues with other initialization. Investigate.");

		}
		GlobalReferenceHandler.ExitAllowEventualReferenceMode();

		GameData.PhysicsWorld = new();
		foreach (Extension extension in Extensions)
		{
			bool alreadySetUp = false;

			if (extension.ExtensionObject is not null &&
				extension.ExtensionObject.SetUpPhysics(out Simulation simulation, out BufferPool bufferPool, out IThreadDispatcher threadDispatcher))
			{
				if (alreadySetUp)
				{
					throw new Exception($"More than one extension attemped to set up the {nameof(PhysicsWorld)}!");
				}

				GameData.PhysicsWorld.SetUp(simulation, bufferPool, threadDispatcher, GameData);

				alreadySetUp = true;
			}
		}
		GameData.PhysicsWorld.Run();

		GameData.UI = UI.CreateAndAdd(GameData);
		GameData.UI.PauseStateChanged += UI_PauseStateChanged;

		GameData.Game = Game.CreateGame(SerializableReference.NewReference(), GameData);
		GameData.Game.GameWorld.StartTicking(this);

		Logging.LogScopeStart("Starting all extensions");
		foreach (Extension extension in Extensions)
		{
			extension.ExtensionObject?.Start(GameData);
		}
		Logging.LogScopeEnd();

		InputTracker.IgnoreNextFrameMousePosition = true;

		StartUpdating();

		GameData.Settings.RegisterToSettingValue<SliderSettingValue>("FoV", (value) => GameData.GraphicsWorld.Camera.FieldOfView = value * DecimalNumber.DegreesToRadians);

		GameData.Settings.RegisterToSettingValue<SliderSettingValue>("Max FPS", (value) => GameData.GraphicsWorld.FrametimeLowerLimit = IntervalRateConversionUnit.Unit / (RateUnit)value.SliderValue);

		GameData.Settings.RegisterToSettingValue<SwitchSettingValue>("Limit FPS", (value) => GameData.GraphicsWorld.ShouldLimitFramerate = value.SwitchValue);

		Logging.LogScopeEnd();
	}

	private void UI_PauseStateChanged(bool paused)
	{
		InputTracker.IgnoreNextFrameMousePosition = true;

		Paused = paused;

		GameData.GraphicsWorld.Window.CaptureMouse = !paused;
	}

	private void StartUpdating()
	{
		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();

			GameData.GraphicsWorld.Run();
			stopwatch.Start();

			TimeUnit timeLastUpdate = stopwatch.Elapsed.TotalSeconds;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (GameData.GraphicsWorld.Window.Exists)
			{
				time = stopwatch.Elapsed.TotalSeconds;

				deltaTime = time - timeLastUpdate;

				CollectUpdatePerformanceData?.Invoke(deltaTime);

				Update((IntervalUnit)DecimalNumber.Max((DN)deltaTime, (DN)Time.UpdateInterval));

				lock (responseSpinner)
					responseSpinner = Time.ResponseSpinner(time);

				Time.WaitIntervalLimit(Time.UpdateInterval, time, stopwatch, out var timeOfContinuation);

				timeLastUpdate = timeOfContinuation;
			}

			// If the Window no longer exists, close the game.
			Environment.Exit(69);
		}))
		{ Name = "Update Thread" };

		thread.Start();
	}

	private void Update(IntervalUnit deltaTime)
	{
		lock (UpdateSyncObject)
		{
			//InputTracker.DeferFurtherInputToNextFrame();

			GameData.UI.Update();

			if (InputTracker.GetKeyDown(Key.P))
			{
				DebugRender.ShouldRender = !DebugRender.ShouldRender;
			}

			if (GameData.UI.InMenu || Paused)
			{
				GameData.GraphicsWorld.Window.CaptureMouse = false;
			}
			else
			{
				GameData.GraphicsWorld.Window.CaptureMouse = true;
			}

			OnUpdate?.Invoke(deltaTime);

			InputTracker.UpdateInputFrame();
		}
	}

	public event Action<IntervalUnit> OnUpdate;

	public void Serialize(string path)
	{
		lock (GameData.Game.GameWorld.TickSyncObject)
		lock (UpdateSyncObject)
		{
			Logging.LogScopeStart($"Serialization started");

			Logging.Log($"Serializing { GameData.Game.GameReferenceHandler.ReferenceCount} references");

			File.Delete(path);

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using FileStream stream = File.OpenWrite(path);

			using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });

			writer.WriteStartDocument();
			{
				writer.SerializeWithoutEmbeddedType(GameData.Game, nameof(Game));
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
		lock (GameData.Game.GameWorld.TickSyncObject)// lock (SynchronizationObject)
		{
			Logging.LogScopeStart($"Deserialization started");

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using Stream stream = File.OpenRead(path);

			using XmlReader reader = XmlReader.Create(stream);

			var serializationData = new SerializationData(GameData);

			//GameData.Game?.Destroy();

			GameData.Game?.GameWorld.Destroy();

			GameData.Game = reader.DeserializeEntitySerializableWithoutEmbeddedType<Game>(serializationData, null, nameof(Game));

			serializationData.SerializationComplete();

			reader.Close();

			stream.Close();

			reader.Dispose();

			stream.Dispose();

			stopwatch.Stop();
			
			Logging.Log($"Deserialized all {GameData.Game.GameReferenceHandler.ReferenceCount} references in {stopwatch.Elapsed.TotalMilliseconds} ms");

			Logging.LogScopeEnd();
		}
	}
}
