using FixedPrecision;
using FXRenderer;
using Space_Refinery_Engine.Audio;
using Space_Refinery_Game_Renderer;
using Space_Refinery_Utilities;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Veldrid;
using static Space_Refinery_Engine.SerializationPaths;

namespace Space_Refinery_Engine;

public sealed class MainGame
{
	private GraphicsWorld GraphicsWorld { get => GameData.GraphicsWorld; set => GameData.GraphicsWorld = value; }
	private AudioWorld AudioWorld { get => GameData.AudioWorld; set => GameData.AudioWorld = value; }
	private PhysicsWorld PhysicsWorld { get => GameData.PhysicsWorld; set => GameData.PhysicsWorld = value; }
	private GameWorld GameWorld { get => GameData.GameWorld; set => GameData.GameWorld = value; }
	private SerializationReferenceHandler ReferenceHandler { get => GameData.ReferenceHandler; set => GameData.ReferenceHandler = value; }
	private Player Player { get; set; }
	private UI UI { get => GameData.UI; set => GameData.UI = value; }
	private Settings Settings { get => GameData.Settings; set => GameData.Settings = value; }

	public GameData GameData { get; private set; }

	public Starfield Starfield { get; private set; }

	public static DebugRender DebugRender;

	public static DebugSettings DebugSettings = new();

	private Window window;

	public bool Paused;

	public readonly object UpdateSyncObject = new();

	public Guid SaveGuid { get; private set; } = Guid.NewGuid();

	public event Action<IntervalUnit> CollectUpdatePerformanceData;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	public static SerializationReferenceHandler GlobalReferenceHandler;

	private static void DeserializeIntoGlobalReferenceHandler(SerializationReferenceHandler globalReferenceHandler, GameData gameData)
	{
		Logging.LogScopeStart("Deserializing into global reference handler");

		var stopwatch = new Stopwatch();
		stopwatch.Start();

		SerializationExtensions.FindAndIndexSerializableTypes(new[] { Assembly.GetExecutingAssembly() });

		var extensions = LoadExtensions(new(gameData, null), globalReferenceHandler);

		SerializationExtensions.FindAndIndexSerializableTypes(extensions);

		List<(Extension, string[] filePaths)> srhFiles = new();

		foreach (var extension in extensions)
		{
			srhFiles.Add((extension, Directory.GetFiles(Path.Combine(extension.ExtensionPath, extension.ExtensionManifest.AssetsPath), $"*{SerializableReferenceHandlerFileExtension}", SearchOption.AllDirectories)));
		}

		List<SerializationData> serializationDatas = new(); // get rid of this when extension context is gone too!

		foreach ((var extension, var files) in srhFiles)
		{
			SerializationData serializationData = new(gameData, extension.AssetsPath);

			serializationDatas.Add(serializationData);

			foreach (var file in files)
			{
				if (file.EndsWith(ExtensionManifestFileExtension))
				{ // Manifest files have already been deserialized earlier when loading extensions and should be not be deserialized twice.
					continue;
				}

				using var individualFileReader = XmlReader.Create(file, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });

				globalReferenceHandler.DeserializeInto(individualFileReader, serializationData, false);
			}
		}

		foreach (var serializationData in serializationDatas)
		{
			serializationData.SerializationComplete();
		}

		stopwatch.Stop();

		Logging.Log($"Deserialized all ({globalReferenceHandler.ReferenceCount}!) global references in {stopwatch.ElapsedMilliseconds} ms");

		Logging.LogScopeEnd();
	}

	public static Extension EngineExtension;

	private static ICollection<Extension> LoadExtensions(SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		Logging.LogScopeStart("Loading extensions");

		Dictionary<string, ExtensionManifest> nameToExtensionManifest = new();
		Dictionary<ExtensionManifest, string> extensionManifestToDirectoryName = new();

		List<string> manifestFilePaths = new();

		manifestFilePaths.AddRange(Directory.GetFiles(AssetsPath, $"*{ExtensionManifestFileExtension}", SearchOption.AllDirectories));
		manifestFilePaths.AddRange(Directory.GetFiles("../../../../Space Refinery Game/bin/Debug/net7.0/_GameAssets", $"*{ExtensionManifestFileExtension}", SearchOption.AllDirectories));

		// Find all extension manifest files and add them to manifestFilePaths,
		// or if there is a directory without any manifest file create a 'No File'-manifest
		// and add it to nameToExtensionManifest and extensionManifestToDirectoryName.
		Directory.CreateDirectory(ModPath);
		foreach (var extensionDirectory in Directory.GetDirectories(ModPath))
		{
			var extensionManifestsInDirectory = Directory.GetFiles(extensionDirectory, $"*{ExtensionManifestFileExtension}", SearchOption.AllDirectories);

			if (extensionManifestsInDirectory.Length == 0)
			{
				var name = Path.GetDirectoryName(extensionDirectory)!;

				var extensionManifest = ExtensionManifest.GenerateNoFileManifest(name, extensionDirectory);

				nameToExtensionManifest.Add(name, extensionManifest);
				extensionManifestToDirectoryName.Add(extensionManifest, Path.GetFullPath(extensionDirectory)!);
				continue;
			}

			manifestFilePaths.Add(extensionManifestsInDirectory[0]);
		}

		Logging.LogAll(manifestFilePaths, "Extension manifest files to be loaded");

		Logging.LogAll(nameToExtensionManifest.Keys, "Additional extension manifest files generated");

		// Deserialize the manifests and add them to nameToExtensionManifest and extensionManifestToDirectoryName.
		referenceHandler.EnterAllowEventualReferenceMode(allowUnresolvedEventualReferences: false);
		{
			foreach (var manifestFilePath in manifestFilePaths)
			{
				using XmlReader reader = XmlReader.Create(manifestFilePath);

				referenceHandler.DeserializeInto<ExtensionManifest>(reader, serializationData, out var extensionManifests);

				foreach (var extensionManifest in extensionManifests)
				{
					nameToExtensionManifest.Add(extensionManifest.ExtensionName, extensionManifest);
					extensionManifestToDirectoryName.Add(extensionManifest, Path.GetFullPath(Path.GetDirectoryName(manifestFilePath)!)!);
				}
			}
		}
		referenceHandler.ExitAllowEventualReferenceMode();

		// Check dependencies for extensions.
		foreach (var extensionManifest in nameToExtensionManifest.Values)
		{
			foreach (var dependency in extensionManifest.Dependencies)
			{
				if (!ExtensionDependency.SatisfiesDependency(dependency, nameToExtensionManifest[dependency.DependedExtension.ExtensionName].ExtensionVersion))
				{
					throw new Exception($"Dependency of {extensionManifest.ExtensionName} to {dependency.DependedExtension.ExtensionName} {dependency.DependencyKind} {dependency.DependencySpecificity} {dependency.ExtensionVersion} could not be satisifed with version {dependency.ExtensionVersion}.");
				}
			}
		}

		// Create extension objects and load their respective assemblies.
		List<Extension> extensions = new();
		foreach (var extensionManifest in nameToExtensionManifest.Values)
		{
			var extension = Extension.CreateAndLoadFromExtensionManifest(extensionManifest, extensionManifestToDirectoryName[extensionManifest]);

			if (extension.ExtensionManifest.SerializableReference == "EngineManifest")
			{
				Debug.Assert(EngineExtension is null);
				EngineExtension = extension;
			}

			extensions.Add(extension);
		}
		Debug.Assert(EngineExtension is not null);

		Logging.LogScopeEnd();

		return extensions;
	}

	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		GameData = new()
		{
			MainGame = this
		};

		this.window = window;

		InputTracker.ListenToWindow(window);

		GraphicsWorld = new();

		GraphicsWorld.SetUp(window, gd, factory, swapchain);

		DebugRender = DebugRender.Create(GraphicsWorld);

		GlobalReferenceHandler = new();
		GlobalReferenceHandler.EnterAllowEventualReferenceMode(false);
		{

			Settings = new(GameData);

			AudioWorld = AudioWorld.Create(GameData);

			AudioWorld.MusicSystem.SetTags(MusicTag.Intense);

			DebugSettings.AccessSetting("Fill music queue", (ActionDebugSetting)AudioWorld.MusicSystem.FillQueue);

			DeserializeIntoGlobalReferenceHandler(GlobalReferenceHandler, GameData);

			Settings.LoadSettingValues();

		}
		GlobalReferenceHandler.ExitAllowEventualReferenceMode();

		PhysicsWorld = new();

		PhysicsWorld.SetUp();

		PhysicsWorld.Run();

		Player = Player.Create(GameData);

		UI = UI.CreateAndAdd(GameData, Player);

		UI.PauseStateChanged += UI_PauseStateChanged;

		GameWorld = new(GameData);

		ReferenceHandler = new();

		Starfield = Starfield.CreateAndAdd(GraphicsWorld);

		Pipe.Create(PipeType.PipeTypes["Straight Pipe"], new Transform(new(0, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), GameData, ReferenceHandler);

		InputTracker.IgnoreNextFrameMousePosition = true;

		//DebugRender.ShouldRender = true;

		StartUpdating();

		GameWorld.StartTicking();

		Settings.RegisterToSettingValue<SliderSettingValue>("FoV", (value) => GraphicsWorld.Camera.FieldOfView = value * DecimalNumber.DegreesToRadians);

		Settings.RegisterToSettingValue<SliderSettingValue>("Max FPS", (value) => GraphicsWorld.FrametimeLowerLimit = IntervalRateConversionUnit.Unit / (RateUnit)value.SliderValue);

		Settings.RegisterToSettingValue<SwitchSettingValue>("Limit FPS", (value) => GraphicsWorld.ShouldLimitFramerate = value.SwitchValue);

		FormatUnit.RegisterToSettings(Settings);
	}

	private void UI_PauseStateChanged(bool paused)
	{
		InputTracker.IgnoreNextFrameMousePosition = true;

		Paused = paused;

		window.CaptureMouse = !paused;
	}

	private void StartUpdating()
	{
		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();

			GraphicsWorld.Run();
			stopwatch.Start();

			TimeUnit timeLastUpdate = stopwatch.Elapsed.TotalSeconds;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (window.Exists)
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
		}))
		{ Name = "Update Thread" };

		thread.Start();
	}

	private void Update(IntervalUnit deltaTime)
	{
		lock (UpdateSyncObject)
		{
			//InputTracker.DeferFurtherInputToNextFrame();

			GraphicsWorld.Camera.Transform = Player.CameraTransform;

			UI.Update();

			if (InputTracker.GetKeyDown(Key.P))
			{
				DebugRender.ShouldRender = !DebugRender.ShouldRender;
			}

			if (UI.InMenu || Paused)
			{
				window.CaptureMouse = false;
			}
			else
			{
				window.CaptureMouse = true;
			}

			if (!Paused && !UI.InMenu)
			{
				Player.Update(deltaTime);
			}

			InputTracker.UpdateInputFrame();
		}
	}

	public void Serialize(string path)
	{
		lock (GameWorld.TickSyncObject)
		lock (UpdateSyncObject)
		{
			Logging.Log($"Serialization started. Serializing {ReferenceHandler.ReferenceCount} references.");

			File.Delete(path);

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using FileStream stream = File.OpenWrite(path);

			using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });

			writer.WriteStartDocument();
			writer.WriteStartElement(nameof(MainGame));
			{
				writer.WriteElementString(nameof(SaveGuid), SaveGuid.ToString());

				Player.Serialize(writer);

				ReferenceHandler.Serialize(writer);
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();

			writer.Flush();
			writer.Close();
			stream.Flush(true);
			stream.Close();
			writer.Dispose();
			stream.Dispose();

			stopwatch.Stop();

			Logging.Log($"Serialized all state in {stopwatch.Elapsed.TotalMilliseconds} ms.");
		}
	}

	public void Deserialize(string path)
	{
		lock (GameWorld.TickSyncObject)// lock (SynchronizationObject)
		{
			Logging.Log($"Deserialization started.");

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using Stream stream = File.OpenRead(path);

			using XmlReader reader = XmlReader.Create(stream);

			var serializationData = new SerializationData(GameData);

			reader.ReadStartElement(nameof(MainGame));
			{
				var newSaveGuid = Guid.Parse(reader.ReadString(nameof(SaveGuid)));

				if (newSaveGuid != SaveGuid)
				{
					Logging.Log($"Deserializing a save with another guid. Guid: {newSaveGuid}");
				}

				SaveGuid = newSaveGuid;

				bool ignoreCleanupTimeFromPerformanceProfiling = DebugSettings.AccessSetting<BooleanDebugSetting>("Ignore cleanup time from performance timing of save loading");

				if (ignoreCleanupTimeFromPerformanceProfiling)
				{
					stopwatch.Stop();
				}

				Player.Destroy();

#if NAÏVE_Reset
				NaïveClear();
#else
				Reset();
#endif
				if (ignoreCleanupTimeFromPerformanceProfiling)
				{
					stopwatch.Start();
				}

				Player = Player.Deserialize(reader, serializationData);

				if (ignoreCleanupTimeFromPerformanceProfiling)
				{
					stopwatch.Stop();
				}

#if !NAÏVE_Reset

				UI.AddToGraphicsWorld();

				Starfield.AddToGraphicsWorld();

				foreach (var pipeType in PipeType.PipeTypes.Values)
				{
					pipeType.BatchRenderable.AddToGraphicsWorld();
				}
#endif
				if (ignoreCleanupTimeFromPerformanceProfiling)
				{
					stopwatch.Start();
				}

				ReferenceHandler = SerializationReferenceHandler.Deserialize(reader, serializationData);
			}
			//reader.ReadEndElement();
			// This line above is disabled because, well, we're never gonna need it, are we?

			serializationData.SerializationComplete();

			reader.Close();

			stream.Close();

			reader.Dispose();

			stream.Dispose();

			stopwatch.Stop();
			
			Logging.Log($"Deserialized all state in {stopwatch.Elapsed.TotalMilliseconds} ms. Deserialized {ReferenceHandler.ReferenceCount} references.");
		}
	}

	/// <summary>
	/// A fast way to reset all systems when loading a save.
	/// </summary>
	private void Reset() // TODO: to allow mods or third party extensions to also clear their stuff, also call an event? Otherwise they would be unable to clear their data when Destroy is not called.
	{ // uh also what about all the subscriptions...? hmmm.. gonna need a lite-destroy method. aw man. (or weak events, HINT HINT MICROSOFT). or maybe just nulling, now that I think abiout it... Hmmmm....
		GameWorld.ResetUnsafe();
		GraphicsWorld.Reset();
		// clear the batch renderables instead? well then again this deals with extras so until scene management like stuff ig....
		AudioWorld.Reset();
		PhysicsWorld.Reset();
	}

	/// <summary>
	/// This method is simply here for comparision and contrast.
	/// </summary>
	private void NaïveClear()
	{
		GameWorld.ClearAll();
	}
}
