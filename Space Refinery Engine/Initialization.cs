using Space_Refinery_Game_Renderer;
using Space_Refinery_Engine.Audio;
using System.Diagnostics;
using System.Xml;
using Veldrid;
using BepuPhysics;
using BepuUtilities.Memory;
using BepuUtilities;

namespace Space_Refinery_Engine;

public sealed class Initialization
{
	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		// TODO: explain initialization dependencies here!

		Logging.LogScopeStart("Game initializing");

		GameData gameData = new();

		InputTracker.ListenToWindow(window);

		gameData.GraphicsWorld = new();
		gameData.GraphicsWorld.SetUp(window, gd, factory, swapchain);
		GameData.DebugRender = DebugRender.Create(gameData.GraphicsWorld);

		gameData.InputUpdate = new(gameData);

		GameData.GlobalReferenceHandler = new();
		GameData.GlobalReferenceHandler.EnterAllowEventualReferenceMode(false);
		{

			gameData.Settings = new(gameData);

			gameData.AudioWorld = AudioWorld.Create(gameData);

			gameData.AudioWorld.MusicSystem.SetTags(MusicTag.Intense);

			GameData.DebugSettings.AccessSetting("Fill music queue", (ActionDebugSetting)gameData.AudioWorld.MusicSystem.FillQueue);

			ResourceDeserialization.DeserializeIntoGlobalReferenceHandler(GameData.GlobalReferenceHandler, gameData, out var extensions);
			gameData.Extensions = extensions;

			gameData.Settings.LoadSettingValuesFromSettingsFile();

			foreach (Extension extension in gameData.Extensions)
			{
				extension.ExtensionObject?.OnGlobalReferenceHandlerDeserialization(GameData.GlobalReferenceHandler, gameData);
			}

			Debug.Assert(GameData.GlobalReferenceHandler.AllowEventualReferences, $"{nameof(gameData.GlobalReferenceHandler.AllowEventualReferences)} mode was disabled at some point deserialization into {nameof(gameData.GlobalReferenceHandler)}. It should not be deactivated as that can cause issues with other initialization. Investigate.");

		}
		GameData.GlobalReferenceHandler.ExitAllowEventualReferenceMode();

		gameData.PhysicsWorld = new();
		foreach (Extension extension in gameData.Extensions)
		{
			bool alreadySetUp = false;

			if (extension.ExtensionObject is not null &&
				extension.ExtensionObject.SetUpPhysics(out Simulation? simulation, out BufferPool? bufferPool, out IThreadDispatcher? threadDispatcher))
			{
				if (alreadySetUp)
				{
					throw new Exception($"Only one extension can set up the {nameof(PhysicsWorld)}!");
				}

				gameData.PhysicsWorld.SetUp(simulation!, bufferPool!, threadDispatcher!, gameData);

				alreadySetUp = true;
			}
		}
		gameData.PhysicsWorld.Run();

		gameData.UI = UI.CreateAndAdd(gameData);
		gameData.UI.PauseStateChanged += gameData.ChangePauseState;

		gameData.Game = Game.CreateGame(SerializableReference.NewReference(), gameData);
		gameData.Game.GameWorld.StartTicking(gameData);

		Logging.LogScopeStart("Starting all extensions");
		foreach (Extension extension in gameData.Extensions)
		{
			extension.ExtensionObject?.Start(gameData);
		}
		Logging.LogScopeEnd();

		InputTracker.IgnoreNextFrameMousePosition = true;

		gameData.InputUpdate.StartUpdating();

		gameData.Settings.RegisterToSettingValue<SliderSettingValue>("FoV", (value) => gameData.GraphicsWorld.Camera.FieldOfView = value * DecimalNumber.DegreesToRadians);
		gameData.Settings.RegisterToSettingValue<SliderSettingValue>("Max FPS", (value) => gameData.GraphicsWorld.FrametimeLowerLimit = IntervalRateConversionUnit.Unit / (RateUnit)value.SliderValue);
		gameData.Settings.RegisterToSettingValue<SwitchSettingValue>("Limit FPS", (value) => gameData.GraphicsWorld.ShouldLimitFramerate = value.SwitchValue);

		Logging.LogScopeEnd();
	}
}
