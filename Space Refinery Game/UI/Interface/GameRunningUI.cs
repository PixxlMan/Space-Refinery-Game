using FixedPrecision;
using ImGuiNET;
using Space_Refinery_Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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

		ImGui.Begin("Center", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
		ImGui.SetWindowPos(new Vector2(width / 2, height / 2), ImGuiCond.Always);
		{
			ImGui.Bullet();
		}
		ImGui.End();

		DoInformationPanel(deltaTime);

		DoHotbar(deltaTime);
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
			ImGui.Columns(pipeTypes.Count);
			for (int i = 0; i < pipeTypes.Count; i++)
			{
				if (pipeTypes[i] is null)
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
					ImGui.TextColored(RgbaFloat.CornflowerBlue.ToVector4(), (pipeTypes[i].Name));
				}
				else
				{
					ImGui.TextDisabled(pipeTypes[i].Name);
				}

				ImGui.NextColumn();
			}
		}
		ImGui.End();
	}
}