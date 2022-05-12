using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System.Diagnostics;
using System.Xml;
using Veldrid;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Game;

public class MainGame
{
	public GraphicsWorld GraphicsWorld;
	public PhysicsWorld PhysicsWorld;
	public GameWorld GameWorld;
	public Player Player;

	public static Settings GlobalSettings;

	public static DebugRender DebugRender;

	public static DebugSettings DebugSettings = new();

	private Window window;
	private UI ui;

	public bool Paused;

	public readonly object SynchronizationObject = new();

	public ChemicalType[] ChemicalTypes;

	public Dictionary<string, ChemicalType> ChemicalTypesDictionary;

	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		GlobalSettings = new();

		GlobalSettings.SetSettingOptions("FoV", new SliderSettingOptions(30, 120));

		GlobalSettings.RegisterToSetting<SliderSetting>("FoV", (se) => GraphicsWorld.Camera.FieldOfView = se.Value * FixedDecimalInt4.DegreesToRadians, defaultValue: new SliderSetting() { Value = 65 });

		ChemicalTypes = ChemicalType.LoadChemicalTypes(Path.Combine(Environment.CurrentDirectory, "Assets", "Chemical types"));

		ChemicalTypesDictionary = ChemicalTypes.ToDictionary((cT) => cT.ChemicalName);

		this.window = window;

		GraphicsWorld = new();

		GraphicsWorld.SetUp(window, gd, factory, swapchain);

		DebugRender = DebugRender.Create(GraphicsWorld);

		PhysicsWorld = new();

		PhysicsWorld.SetUp();

		PhysicsWorld.Run(); 

		ui = UI.Create(GraphicsWorld, this);

		ui.PauseStateChanged += UI_PauseStateChanged;

		GameWorld = new(this);

		Player = Player.Create(this, PhysicsWorld, GraphicsWorld, GameWorld, ui);

		Starfield.Create(GraphicsWorld);

		GameWorld.AddConstruction(Pipe.Create(ui.SelectedPipeType, new Transform(new(0, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), ui, PhysicsWorld, GraphicsWorld, GameWorld, this));

		InputTracker.IgnoreNextFrameMousePosition = true;

		GraphicsWorld.Run();

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
		lock(SynchronizationObject) lock(GraphicsWorld.SynchronizationObject)
		{
			GraphicsWorld.Camera.Transform = Player.CameraTransform;

			window.PumpEvents(out var input); // TODO: modify to only pump input related events and let renderer pump drawing related?

			InputTracker.UpdateFrameInput(input);

			ui.Update();

			if (InputTracker.GetKeyDown(Key.P))
			{
				DebugRender.ShouldRender = !DebugRender.ShouldRender;
			}

			if (ui.InMenu || Paused)
			{
				window.CaptureMouse = false;
			}
			else
			{
				window.CaptureMouse = true;
			}

			if (!Paused && !ui.InMenu)
			{
				Player.Update(deltaTime);
			}
		}
	}

	public void Serialize(string path)
	{
		File.Delete(path);

		using Stream stream = File.OpenWrite(path);

		using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });

		writer.WriteStartDocument();
		writer.WriteStartElement(nameof(MainGame));
		{
			Player.Serialize(writer);

			GameWorld.SerializeConstructions(writer);
		}
		writer.WriteEndElement();
		writer.WriteEndDocument();

		writer.Flush();

		writer.Close();

		stream.Close();
	}

	public void Deserialize(string path)
	{
		using Stream stream = File.OpenRead(path);

		using XmlReader reader = XmlReader.Create(stream);

		reader.ReadStartElement(nameof(MainGame));
		{
			Player = Player.Deserialize(reader, this, PhysicsWorld, GraphicsWorld, GameWorld, ui);

			GameWorld.DeserializeConstructions(reader);
		}
		//reader.ReadEndElement();

		reader.Close();

		stream.Close();
	}
}
