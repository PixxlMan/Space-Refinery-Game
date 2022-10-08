﻿using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System.Diagnostics;
using System.Xml;
using Veldrid;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Game;

public sealed class MainGame
{
	private GraphicsWorld GraphicsWorld { get => GameData.GraphicsWorld; set => GameData.GraphicsWorld = value; }
	private PhysicsWorld PhysicsWorld { get => GameData.PhysicsWorld; set => GameData.PhysicsWorld = value; }
	private GameWorld GameWorld { get => GameData.GameWorld; set => GameData.GameWorld = value; }
	private SerializationReferenceHandler ReferenceHandler { get => GameData.ReferenceHandler; set => GameData.ReferenceHandler = value; }
	private Player Player { get; set; }
	private UI UI { get => GameData.UI; set => GameData.UI = value; }

	public GameData GameData { get; private set; }

	public static SerializationReferenceHandler GlobalReferenceHandler;

	private static SerializationReferenceHandler DeserializeGlobalReferenceHandler(SerializationData serializationData)
	{
		using var reader = XmlReader.Create(Path.Combine(Environment.CurrentDirectory, "Assets", "GlobalReferences.xml"));

		reader.ReadStartElement("GlobalReferences");

		var globalReferenceHanlder = SerializationReferenceHandler.Deserialize(reader, serializationData);

		serializationData.SerializationComplete();

		reader.ReadEndElement();

		return globalReferenceHanlder;
	}

	public static Settings GlobalSettings;

	public static DebugRender DebugRender;

	public static DebugSettings DebugSettings = new();

	private Window window;

	public bool Paused;

	public readonly object SynchronizationObject = new();

	public ChemicalType[] ChemicalTypes;

	public static Dictionary<string, ChemicalType> ChemicalTypesDictionary;

	public PipeType[] PipeTypes;

	public Guid SaveGuid { get; private set; } = Guid.NewGuid();

	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		GameData = new(null, null, null, null, this, null);

		GlobalSettings = new();

		GlobalSettings.SetSettingOptions("FoV", new SliderSettingOptions(30, 120, "degrees"));

		GlobalSettings.RegisterToSetting<SliderSetting>("FoV", (se) => GraphicsWorld.Camera.FieldOfView = se.Value * DecimalNumber.DegreesToRadians, defaultValue: new SliderSetting() { Value = 65 });

		GlobalSettings.SetSettingOptions("Max framerate", new SliderSettingOptions(1, 1000, "FPS"));

		GlobalSettings.RegisterToSetting<SliderSetting>("Max framerate", (se) => GraphicsWorld.FrametimeLowerLimit = 1 / se.Value, defaultValue: new SliderSetting() { Value = 500 });

		GlobalSettings.SetSettingOptions("Limit framerate", null);

		GlobalSettings.RegisterToSetting<BooleanSetting>("Limit framerate", (bs) => GraphicsWorld.ShouldLimitFramerate = bs.Value, defaultValue: new BooleanSetting() { Value = true });

		ChemicalTypes = ChemicalType.LoadChemicalTypes(Path.Combine(Environment.CurrentDirectory, "Assets", "Chemical types"));

		ChemicalTypesDictionary = ChemicalTypes.ToDictionary((cT) => cT.ChemicalName);

		this.window = window;

		GraphicsWorld = new();

		GraphicsWorld.SetUp(window, gd, factory, swapchain);

		

		DebugRender = DebugRender.Create(GraphicsWorld);

		if (GlobalReferenceHandler is null)
		{
			GlobalReferenceHandler = DeserializeGlobalReferenceHandler(new SerializationData(GameData));
		}

		PhysicsWorld = new();

		PhysicsWorld.SetUp();

		PhysicsWorld.Run();

		UI = UI.Create(GameData);

		UI.PauseStateChanged += UI_PauseStateChanged;

		GameWorld = new(GameData);

		ReferenceHandler = new();

		Player = Player.Create(GameData);

		Starfield.Create(GraphicsWorld);

		GameWorld.AddConstruction(Pipe.Create(UI.SelectedPipeType, new Transform(new(0, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), GameData, ReferenceHandler));

		InputTracker.IgnoreNextFrameMousePosition = true;

		//DebugRender.ShouldRender = true;

		StartUpdating();

		GameWorld.StartTicking();
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

			FixedDecimalInt4 timeLastUpdate = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalInt4>();
			FixedDecimalInt4 time;
			FixedDecimalInt4 deltaTime;
			while (window.Exists)
			{
				time = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalInt4>();

				deltaTime = time - timeLastUpdate;

				timeLastUpdate = time;

				Thread.Sleep((Time.UpdateInterval * 1000).ToInt32());

				Update(deltaTime);
			}
		}))
		{ Name = "Update Thread" };

		thread.Start();
	}

	private void Update(FixedDecimalInt4 deltaTime)
	{
		lock (SynchronizationObject) lock (GraphicsWorld.SynchronizationObject)
		{
			GraphicsWorld.Camera.Transform = Player.CameraTransform;

			window.PumpEvents(out var input); // TODO: modify to only pump input related events and let renderer pump drawing related?

			InputTracker.UpdateFrameInput(input);

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
		}
	}

	public void Serialize(string path)
	{
		lock (GameWorld.TickSyncObject)	lock (SynchronizationObject)
		{
			Console.WriteLine($"Serialization started. Serializing {ReferenceHandler.ReferenceCount} references.");

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

			Console.WriteLine($"Serialized all state in {stopwatch.Elapsed.TotalMilliseconds} ms.");
		}
	}

	public void Deserialize(string path)
	{
		lock (GameWorld.TickSyncObject) lock (SynchronizationObject)
		{
			Console.WriteLine($"Deserialization started.");

			Stopwatch stopwatch = new();

			stopwatch.Start();

			using Stream stream = File.OpenRead(path);

			using XmlReader reader = XmlReader.Create(stream);

			var serializationData = new SerializationData(GameData);

			reader.ReadStartElement(nameof(MainGame));
			{
				var newSaveGuid = reader.ReadReferenceGUID(nameof(SaveGuid));

				if (newSaveGuid != SaveGuid)
				{
					Console.WriteLine($"Deserializing a save with another guid. Guid: {newSaveGuid}");
				}

				SaveGuid = newSaveGuid;

				Player.Destroy();

				Player = Player.Deserialize(reader, serializationData);

				GameWorld.ClearAll();

				ReferenceHandler = SerializationReferenceHandler.Deserialize(reader, serializationData);
			}
			//reader.ReadEndElement();

			serializationData.SerializationComplete();

			reader.Close();

			stream.Close();

			reader.Dispose();

			stream.Dispose();

			stopwatch.Stop();
			
			Console.WriteLine($"Deserialized all state in {stopwatch.Elapsed.TotalMilliseconds} ms. Deserialized {ReferenceHandler.ReferenceCount} references.");
		}
	}
}
