using FixedPrecision;
using ImGuiNET;
using Space_Refinery_Game_Renderer;
using Space_Refinery_Utilities;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Engine;

partial class UI
{
	public static Action DoDebugStatusUI;

	private void DoStatus()
	{
		ImGui.Begin("Status", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
		ImGui.SetWindowPos(new Vector2(0, 0), ImGuiCond.Always);
		ImGui.SetWindowSize(new Vector2(width, height));
		{
			if (MainGame.DebugRender.ShouldRender)
			{
				ImGui.TextColored(RgbaFloat.Red.ToVector4(), "Debug drawing");
			}

			if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Display performance information"))
			{
				DoPerformanceInfo();
			}
			else if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("System response spinners", true))
			{
				ImGui.TextUnformatted(gameData.GraphicsWorld.ResponseSpinner.ToString());
			}

			if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Display elapsed time and ticks", false))
			{
				ImGui.Text($"Tick time: {Time.CurrentTickTime}");
				ImGui.Text($"Tick: {Time.TicksElapsed}");
			}

			DoDebugStatusUI?.Invoke();
		}
		ImGui.End();
	}

	private static readonly Portion<TimeUnit> budgetUseYellowThreshold = 0.75;

	private void DoPerformanceInfo()
	{
		ImGui.Columns(3);
		ImGui.SetColumnWidth(0, 150);
		ImGui.SetColumnWidth(1, 120);
		ImGui.SetColumnWidth(2, 200);
		{
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"Frame time:");
			RgbaFloat tickColor;
			if (gameData.PerformanceStatisticsCollector.TickBudgetUse > 1)
			{
				tickColor = RgbaFloat.Red;
			}
			else if (gameData.PerformanceStatisticsCollector.TickBudgetUse > budgetUseYellowThreshold)
			{
				tickColor = RgbaFloat.Yellow;
			}
			else
			{
				tickColor = RgbaFloat.Green;
			}
			ImGui.TextColored(tickColor.ToVector4(), $"Tick time:");
			RgbaFloat updateColor;
			if (gameData.PerformanceStatisticsCollector.UpdateBudgetUse > 1)
			{
				updateColor = RgbaFloat.Red;
			}
			else if (gameData.PerformanceStatisticsCollector.UpdateBudgetUse > budgetUseYellowThreshold)
			{
				updateColor = RgbaFloat.Yellow;
			}
			else
			{
				updateColor = RgbaFloat.Green;
			}
			ImGui.TextColored(updateColor.ToVector4(), $"Update time:");
			RgbaFloat physicsUpdateColor;
			if (gameData.PerformanceStatisticsCollector.PhysicsBudgetUse > 1)
			{
				physicsUpdateColor = RgbaFloat.Red;
			}
			else if (gameData.PerformanceStatisticsCollector.PhysicsBudgetUse > budgetUseYellowThreshold)
			{
				physicsUpdateColor = RgbaFloat.Yellow;
			}
			else
			{
				physicsUpdateColor = RgbaFloat.Green;
			}
			ImGui.TextColored(physicsUpdateColor.ToVector4(), $"Physics update time:");
		}
		ImGui.NextColumn();
		{
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{(DN)gameData.PerformanceStatisticsCollector.RendererFrameTime * 1000} ms");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{(DN)gameData.PerformanceStatisticsCollector.TickTime * 1000} ms");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{(DN)gameData.PerformanceStatisticsCollector.UpdateTime * 1000} ms");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{(DN)gameData.PerformanceStatisticsCollector.PhysicsTime * 1000} ms");
		}
		ImGui.NextColumn();
		{
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.RendererFramerate} FPS) {gameData.GraphicsWorld.ResponseSpinner}");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.TicksPerSecond} TPS) {gameData.Game.GameWorld.ResponseSpinner}");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.UpdatesPerSecond} UPS) {gameData.MainGame.ResponseSpinner}");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.PhysicsUpdatesPerSecond} PUPS) {gameData.PhysicsWorld.ResponseSpinner}");
		}
		ImGui.NextColumn();
	}

	private void DoUIOfDisparateDebuggingMenus()
	{
		if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show miscellaneous debugging menus"))
			return;

		BatchRenderable.DoDebugUI();

		gameData.Game.GameWorld.DoDebugUI();

		if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show dot at {0, 0, 0}"))
		{
			var pos = gameData.GraphicsWorld.Camera.WorldPointToScreenPoint(Vector3FixedDecimalInt4.UnitY, Size, out var visible).ToVector2();

			if (visible)
			{
				drawList.AddCircleFilled(pos, 10, uint.MaxValue);
			}

			Logging.Log("Is dot visible: " + visible.ToString());
		}
	}
}
