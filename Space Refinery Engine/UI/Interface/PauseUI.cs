using FixedPrecision;
using ImGuiNET;
using System.Numerics;

namespace Space_Refinery_Engine;

partial class UI
{
	SaveCreatorUIPrototype saveCreator = new();

	private void DoDebugSettingsUI()
	{
		lock (syncRoot)
		{
			if (ImGui.Begin("Debug Settings", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
			{
				ImGui.SetWindowSize(debugSettingsMenuSize, ImGuiCond.Always);
				ImGui.SetWindowPos(new Vector2((width / 2 - pauseMenuSize.X / 2) + pauseMenuSize.X, height / 2 - debugSettingsMenuSize.Y / 2), ImGuiCond.Always);

				MainGame.DebugSettings.DoDebugSettingsUI();

				ImGui.End();

				saveCreator.DoUI();
			}
		}
	}

	private Vector2 debugSettingsMenuSize => new Vector2((width / 3) * 0.9f, (height / 10) * 8);

	private Vector2 pauseMenuSize => new Vector2(width / 3, (height / 10) * 8);

	private Vector2 settingsMenuSize => new Vector2(width / 2, (height / 10) * 8);

	bool inSettings;

	private void DoPauseMenuUI(FixedDecimalLong8 deltaTime)
	{
		ImGui.Begin("Pause menu", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
		ImGui.SetWindowPos(new Vector2(width / 2 - pauseMenuSize.X / 2, height / 2 - pauseMenuSize.Y / 2), ImGuiCond.Always);
		ImGui.SetWindowSize(pauseMenuSize, ImGuiCond.Always);
		{
			if (ImGui.Button("Resume"))
			{
				Unpause();
			}

			if (ImGui.Button("Save"))
			{
				Task.Run(() =>
				{
					lock (gameData.GameWorld.TickSyncObject)
					{
						gameData.MainGame.Serialize(@"R:\save.xml");
					}
				});
			}

			if (ImGui.Button("Load"))
			{
				Task.Run(() =>
				{
					lock (gameData.GameWorld.TickSyncObject)// lock (gameData.GameWorld.SynchronizationObject)
					{
						gameData.MainGame.Deserialize(@"R:\save.xml");
					}
				});
			}

			if (ImGui.Button("Settings"))
			{
				inSettings = !inSettings;
			}

			if (inSettings)
			{
				inSettings = ImGui.Begin("Settings", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
				ImGui.SetWindowPos(new Vector2(width / 2 - settingsMenuSize.X / 2, height / 2 - settingsMenuSize.Y / 2), ImGuiCond.Always);
				ImGui.SetWindowSize(settingsMenuSize, ImGuiCond.Always);
				{
					gameData.Settings.DoSettingsUI();
				}
				ImGui.SetWindowCollapsed(false);
				ImGui.End();
			}

			if (ImGui.Button("Exit game"))
			{
				GC.WaitForPendingFinalizers();
				Environment.Exit(69);
			}
		}
		ImGui.End();

		DoStatus();

		DoDebugSettingsUI();

		DoUIOfDisparateDebuggingMenus();
	}
}
