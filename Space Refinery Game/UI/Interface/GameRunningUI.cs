﻿using FixedPrecision;
using ImGuiNET;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game;

partial class UI
{
	private void DoGameRunningUI(FixedDecimalLong8 deltaTime)
	{
		if (InMenu)
		{
			if (ImGui.Begin(MenuTitle, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
			{
				ImGui.SetWindowPos(new Vector2(width / 2 - ImGui.GetWindowSize().X / 2, height / 2 - ImGui.GetWindowSize().Y / 2), ImGuiCond.Always);
				{
					doMenu();
					//InMenu = !ImGui.Button("Close");
				}
				ImGui.End();
			}
			else
			{
				InMenu = false;
			}
		}

		DoStatus();

		DoHotbar(deltaTime);

		DoInformationPanel(deltaTime);

		DrawCrosshair();

		if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show miscellaneous debugging menus") && MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show miscellaneous debugging menus in game"))
		{
			DoUIOfDisparateDebuggingMenus();
		}
	}

	private void DrawCrosshair()
	{
		var draw = ImGui.GetBackgroundDrawList();
		draw.AddCircle(new Vector2(width / 2, height / 2), 6, /*Hacky white*/uint.MaxValue);
	}

	private DecimalNumber informationPanelFading = 1;

	private void DoInformationPanel(DecimalNumber deltaTime)
	{
		informationPanelFading += 1 * /*informationPanelFading **/ deltaTime * (CurrentlySelectedInformationProvider is null ? -1 : 4);
		informationPanelFading = DecimalNumber.Clamp(informationPanelFading, 0, 1);
		ImGui.SetNextWindowBgAlpha((float)informationPanelFading);
		ImGui.Begin("Information panel", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs);
		ImGui.SetWindowPos(new Vector2((width / 4 * 3)/* - ImGui.GetWindowSize().X / 2*/, (height / 2) - ImGui.GetWindowSize().Y / 2), ImGuiCond.Always);
		//ImGui.SetWindowSize();
		{
			if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show player info"))
			{
				ImGui.TextDisabled("Information for: Player");
				ImGui.Text("Connector: " + ConnectorSelection);
				ImGui.Text("Entity: " + EntitySelection);
				ImGui.Text("Rotation: " + RotationIndex);
				ImGui.Separator();
			}

			if (CurrentlySelectedInformationProvider is not null)
			{
				ImGui.TextDisabled($"Information for: {CurrentlySelectedInformationProvider.Name}");
				CurrentlySelectedInformationProvider.InformationUI();
			}
			else
			{
				ImGui.TextDisabled("Nothing to view information for.");
			}
		}
		ImGui.End();
	}

	private void DoHotbar(FixedDecimalLong8 deltaTime)
	{
		ImGui.SetNextWindowBgAlpha((float)DecimalNumber.Max(hotbarFading, 0.35));
		hotbarFading -= hotbarFading * 0.3 * (DecimalNumber)deltaTime;
		ImGui.Begin("Hotbar", /*ImGuiWindowFlags.AlwaysAutoResize | */ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
		ImGui.SetWindowPos(new Vector2(width / 2 - ImGui.GetWindowWidth() / 2, height / 5 * 4 - ImGui.GetWindowHeight() / 2), ImGuiCond.Always);
		ImGui.SetWindowSize(new Vector2(500, 50), ImGuiCond.Always);
		{
			ImGui.Columns(hotbarItems.Count);
			for (int i = 0; i < hotbarItems.Count; i++)
			{
				if (hotbarItems[i] is null)
				{
					if (EntitySelection == i)
					{
						ImGui.TextColored(RgbaFloat.CornflowerBlue.ToVector4(), "None");
					}
					else
					{
						ImGui.TextDisabled("None");
					}

					ImGui.NextColumn();
					continue;
				}

				if (EntitySelection == i)
				{
					ImGui.TextColored(RgbaFloat.CornflowerBlue.ToVector4(), (hotbarItems[i].Name));
				}
				else
				{
					ImGui.TextDisabled(hotbarItems[i].Name);
				}

				ImGui.NextColumn();
			}
		}
		ImGui.End();
	}
}