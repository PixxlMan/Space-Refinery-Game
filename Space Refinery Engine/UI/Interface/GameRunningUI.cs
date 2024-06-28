using FixedPrecision;
using ImGuiNET;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Engine;

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

		DrawCrosshair();

		DoInformationPanel(deltaTime);

		if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Show miscellaneous debugging menus") && GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Show miscellaneous debugging menus in game"))
		{
			DoUIOfDisparateDebuggingMenus();
		}
	}

	private void DrawCrosshair()
	{
		drawList.AddCircle(new Vector2(width / 2, height / 2), 6, /*Hacky white*/uint.MaxValue);
	}

	private DecimalNumber informationPanelFading = 1;

	private PhysicsObject? lastLookedAtPhysicsObject;

	private PhysicsObject? currentlyLookedAtPhysicsObject;

	// Magically set by Player.
	internal PhysicsObject? CurrentlyLookedAtPhysicsObject
	{
		get
		{
			lock (syncRoot)
				return currentlyLookedAtPhysicsObject;
		}
		set
		{
			lock (syncRoot)
				currentlyLookedAtPhysicsObject = value;
		}
	}

	private IInformationProvider? currentlySelectedInformationProvider;
	// Magically set by Player.
	internal IInformationProvider? CurrentlySelectedInformationProvider { get { lock (syncRoot) return currentlySelectedInformationProvider; } set { lock (syncRoot) currentlySelectedInformationProvider = value; } }
	/*TODO: do locking on each individual objects themselves instead of using SyncRoot (performance, mainly)? and of course get rid of the field then - all accesses have to be locked! even from within this class*/

	private IInformationProvider? relevantInformationProvider;

	private void DoInformationPanel(DecimalNumber deltaTime)
	{
		relevantInformationProvider = CurrentlySelectedInformationProvider ?? (informationPanelFading != 0 ? relevantInformationProvider : null); // Only update relevantInformationProvider the new CurrentlySelectedInformationProvider is not null

		informationPanelFading += 1 * /*informationPanelFading **/ deltaTime * (CurrentlySelectedInformationProvider is null ? -1 : 4);
		informationPanelFading = DecimalNumber.Clamp(informationPanelFading, 0, 1);

		Vector2FixedDecimalInt4 panelLocation;

		// Lock to ensure LookedAtPhysicsObject isn't destroyed while being used.
		lock (gameData.PhysicsWorld.SyncRoot)
		{
			if (relevantInformationProvider is null || CurrentlyLookedAtPhysicsObject is null || !CurrentlyLookedAtPhysicsObject.Valid)
			{
				if (informationPanelFading != 0 && lastLookedAtPhysicsObject is not null && lastLookedAtPhysicsObject.Valid)
				{
					panelLocation = gameData.GraphicsWorld.Camera.WorldPointToScreenPoint(lastLookedAtPhysicsObject.Transform.Position, Size, out _);
				}
				else
				{
					panelLocation = new Vector2FixedDecimalInt4((width / 4 * 3)/* - ImGui.GetWindowSize().X / 2*/, (height / 2) - ImGui.GetWindowSize().Y / 2);
				}
			}
			else
			{
				panelLocation = gameData.GraphicsWorld.Camera.WorldPointToScreenPoint(CurrentlyLookedAtPhysicsObject.Transform.Position, Size, out _ /*since the values will clamp to the edges of the screen, we don't need to do anything*/);

				lastLookedAtPhysicsObject = CurrentlyLookedAtPhysicsObject;
			}
		}

		// TODO: add if (looking at but not visible) -> place at middle! - to ensure visibility when inside object?)

		if (relevantInformationProvider is not null && !GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Show player info") && informationPanelFading != 0)
		{
			ImGui.SetNextWindowBgAlpha((float)informationPanelFading);
			ImGui.Begin("Information panel", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs);
			ImGui.SetWindowPos(panelLocation.ToVector2(), ImGuiCond.Always);
			//ImGui.SetWindowSize();
			{
				if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Show player info"))
				{
					ImGui.TextDisabled("Information for: Player");
					ImGui.Text("Connector: " + ConnectorSelection);
					ImGui.Text("Entity: " + EntitySelection);
					ImGui.Text("Rotation: " + RotationIndex);
					ImGui.Separator();
				}

				if (relevantInformationProvider is not null)
				{
					ImGui.TextDisabled($"{relevantInformationProvider.Name}");
					relevantInformationProvider.InformationUI();
				}
			}
			ImGui.End();
		}
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
					ImGui.TextColored(RgbaFloat.CornflowerBlue.ToVector4(), (hotbarItems[i]!.Name));
				}
				else
				{
					ImGui.TextDisabled(hotbarItems[i]!.Name);
				}

				ImGui.NextColumn();
			}
		}
		ImGui.End();
	}
}