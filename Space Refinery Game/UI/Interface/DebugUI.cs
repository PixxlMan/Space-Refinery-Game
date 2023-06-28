using ImGuiNET;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game;

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

			DoDebugStatusUI?.Invoke();
		}
		ImGui.End();
	}

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
			else if (gameData.PerformanceStatisticsCollector.TickBudgetUse > (DecimalNumber)0.75)
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
			else if (gameData.PerformanceStatisticsCollector.UpdateBudgetUse > (DecimalNumber)0.75)
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
			else if (gameData.PerformanceStatisticsCollector.PhysicsBudgetUse > (DecimalNumber)0.75)
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
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{gameData.PerformanceStatisticsCollector.RendererFrameTime * 1000} ms");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{gameData.PerformanceStatisticsCollector.TickTime * 1000} ms");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{gameData.PerformanceStatisticsCollector.UpdateTime * 1000} ms");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"{gameData.PerformanceStatisticsCollector.PhysicsTime * 1000} ms");
		}
		ImGui.NextColumn();
		{
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.RendererFramerate} FPS) {gameData.GraphicsWorld.ResponseSpinner}");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.TicksPerSecond} TPS) {gameData.GameWorld.ResponseSpinner}");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.UpdatesPerSecond} UPS) {gameData.MainGame.ResponseSpinner}");
			ImGui.TextColored(RgbaFloat.White.ToVector4(), $"({gameData.PerformanceStatisticsCollector.PhysicsUpdatesPerSecond} PUPS) {gameData.PhysicsWorld.ResponseSpinner}");
		}
		ImGui.NextColumn();
	}

	private static void DoUIOfDisparateDebuggingMenus()
	{
		if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show miscellaneous debugging menus"))
			return;

		BatchRenderable.DoDebugUI();
	}
}
