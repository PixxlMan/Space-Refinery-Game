﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veldrid;
using ImGuiNET;
using FixedPrecision;
using System.Numerics;
using FXRenderer;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class UI : IRenderable
	{
		public IInformationProvider CurrentlySelectedInformationProvider;

		private ImGuiRenderer imGuiRenderer;

		private GraphicsDevice gd;

		private MainGame mainGame;

		public List<PipeType> PipeTypes = new();

		public PipeType SelectedPipeType => PipeTypes[EntitySelection];

		public int EntitySelection;

		public bool Paused;

		public int RotationIndex;

		public event Action<bool> PauseStateChanged;

		public event Action<IEntityType> SelectedEntityTypeChanged;

		public void ChangeEntitySelection(int selectionDelta)
		{
			EntitySelection += selectionDelta;
			 
			while (EntitySelection >= PipeTypes.Count || EntitySelection < 0)
			{
				if (EntitySelection < 0)
				{
					EntitySelection += PipeTypes.Count;
				}
				else if (EntitySelection >= PipeTypes.Count)
				{
					EntitySelection -= PipeTypes.Count;
				}
			}

			ChangeConnectorSelection(0);

			SelectedEntityTypeChanged?.Invoke(SelectedPipeType);
		}

		public int ConnectorSelection;

		public void ChangeConnectorSelection(int selectionDelta)
		{
			ConnectorSelection += selectionDelta;

			while (ConnectorSelection >= SelectedPipeType.ConnectorPlacements.Length || ConnectorSelection < 0)
			{
				if (ConnectorSelection < 0)
				{
					ConnectorSelection += SelectedPipeType.ConnectorPlacements.Length;
				}
				else if (ConnectorSelection >= SelectedPipeType.ConnectorPlacements.Length)
				{
					ConnectorSelection -= SelectedPipeType.ConnectorPlacements.Length;
				}
			}
		}

		private UI(GraphicsDevice gd)
		{
			imGuiRenderer = new(gd, gd.MainSwapchain.Framebuffer.OutputDescription, (int)gd.MainSwapchain.Framebuffer.Width, (int)gd.MainSwapchain.Framebuffer.Height);

			imGuiRenderer.CreateDeviceResources(gd, gd.MainSwapchain.Framebuffer.OutputDescription);

			this.gd = gd;
		}

		public bool InMenu;

		private Action doMenu;

		public string MenuTitle;

		public void EnterMenu(Action doMenu, string title)
		{
			InMenu = true;

			this.doMenu = doMenu;

			MenuTitle = title;

			ImGui.Begin(title);
			{
				ImGui.SetWindowCollapsed(false);
			}
			ImGui.End();
		}

		public static UI Create(GraphicsWorld graphWorld, MainGame mainGame)
		{
			ImGui.CreateContext();

			UI ui = new(graphWorld.GraphicsDevice);

			graphWorld.AddRenderable(ui, 1);

			ui.PipeTypes.AddRange(PipeType.PipeTypes.Values);

			ui.Style();

			ui.mainGame = mainGame;

			return ui;
		}

		public void AddDrawCommands(CommandList cl)
		{
			imGuiRenderer.Update(1, InputTracker.FrameSnapshot);

			DoUI();

			imGuiRenderer.WindowResized((int)gd.MainSwapchain.Framebuffer.Width, (int)gd.MainSwapchain.Framebuffer.Height);

			imGuiRenderer.Render(gd, cl);
		}

		public void Update()
		{
			if (InputTracker.GetKeyDown(Key.C) && InputTracker.GetKey(Key.ShiftLeft))
			{
				ChangeConnectorSelection(-1);
			}
			else if (InputTracker.GetKeyDown(Key.C))
			{
				ChangeConnectorSelection(1);
			}

			if (InputTracker.GetKeyDown(Key.R) && InputTracker.GetKey(Key.ShiftLeft))
			{
				RotationIndex--;
			}
			else if (InputTracker.GetKeyDown(Key.R))
			{
				RotationIndex++;
			}

			if (InputTracker.FrameSnapshot.WheelDelta != 0)
			{
				ChangeEntitySelection(-(int)InputTracker.FrameSnapshot.WheelDelta);
			}

			if (InMenu && InputTracker.CaptureKeyDown(Key.F))
			{
				InMenu = false;
			}

			if (InputTracker.GetKeyDown(Key.Escape))
			{
				if (InMenu)
				{
					InMenu = false;
				}
				else
				{
					TogglePause();
				}
			}
		}

		public void TogglePause()
		{
			if (Paused)
			{
				Unpause();
			}
			else
			{
				Pause();
			}
		}

		public void Pause()
		{
			if (Paused)
			{
				return;
			}

			Paused = true;

			PauseStateChanged?.Invoke(true);
		}

		public void Unpause()
		{
			if (!Paused)
			{
				return;
			}

			Paused = false;

			inSettings = false;

			PauseStateChanged?.Invoke(false);
		}

		public void DoUI()
		{
			if (!Paused)
			{
				DoGameRunningUI();
			}
			else if (Paused)
			{
				DoPauseMenuUI();
			}
		}

		private void DoDebugSettingsUI()
		{
			if (ImGui.Begin("Debug Settings", ImGuiWindowFlags.AlwaysAutoResize))
			{
				foreach (var debugSetting in MainGame.DebugSettings.DebugSettingsDictionary.Values)
				{
					debugSetting.DrawUIElement();
				}

				ImGui.End();
			}
		}

		private Vector2 pauseMenuSize => new Vector2(gd.MainSwapchain.Framebuffer.Width / 3, (gd.MainSwapchain.Framebuffer.Height / 10) * 8);

		private Vector2 settingsMenuSize => new Vector2(gd.MainSwapchain.Framebuffer.Width / 2, (gd.MainSwapchain.Framebuffer.Height / 10) * 8);

		bool inSettings;

		private void DoPauseMenuUI()
		{
			ImGui.Begin("Pause menu", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2 - pauseMenuSize.X / 2, gd.MainSwapchain.Framebuffer.Height / 2 - pauseMenuSize.Y / 2), ImGuiCond.Always);
			ImGui.SetWindowSize(pauseMenuSize, ImGuiCond.Always);
			{
				if (ImGui.Button("Resume"))
				{
					Unpause();
				}

				if (ImGui.Button("Save"))
				{
					mainGame.Serialize(@"R:\save.xml");
				}

				if (ImGui.Button("Load"))
				{
					lock (mainGame.GameWorld.TickSyncObject)
					{
						mainGame.Deserialize(@"R:\save.xml");
					}
				}

				if (ImGui.Button("Settings"))
				{
					inSettings = !inSettings;
				}

				if (inSettings)
				{
					inSettings = ImGui.Begin("Settings", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
					ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2 - settingsMenuSize.X / 2, gd.MainSwapchain.Framebuffer.Height / 2 - settingsMenuSize.Y / 2), ImGuiCond.Always);
					ImGui.SetWindowSize(settingsMenuSize, ImGuiCond.Always);
					{
						MainGame.GlobalSettings.DoSettingsUI();
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

			DoDebugSettingsUI();
		}

		private void DoGameRunningUI()
		{
			if (InMenu)
			{
				if (ImGui.Begin(MenuTitle, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
				{
					ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2 - ImGui.GetWindowSize().X / 2, gd.MainSwapchain.Framebuffer.Height / 2 - ImGui.GetWindowSize().Y / 2), ImGuiCond.Always);
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

			ImGui.Begin("Status", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
			ImGui.SetWindowPos(new Vector2(0, 0), ImGuiCond.Always);
			{
				if (MainGame.DebugRender.ShouldRender)
				{
					ImGui.TextColored(RgbaFloat.Red.ToVector4(), "Debug drawing");
				}
			}
			ImGui.End();

			ImGui.Begin("Center", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2, gd.MainSwapchain.Framebuffer.Height / 2), ImGuiCond.Always);
			{
				ImGui.Bullet();
			}
			ImGui.End();

			ImGui.Begin("Information panel", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs);
			ImGui.SetWindowPos(new Vector2((gd.MainSwapchain.Framebuffer.Width / 4 * 3)/* - ImGui.GetWindowSize().X / 2*/, (gd.MainSwapchain.Framebuffer.Height / 2) - ImGui.GetWindowSize().Y / 2), ImGuiCond.Always);
			{
				if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show player info"))
				{
					ImGui.Text("Connector: " + ConnectorSelection);
					ImGui.Text("Entity: " + EntitySelection);
					ImGui.Text("Rotation: " + RotationIndex);
				}

				if (CurrentlySelectedInformationProvider is not null)
				{
					ImGui.Text($"Information for: {CurrentlySelectedInformationProvider.Name}");
					CurrentlySelectedInformationProvider.InformationUI();
				}
				else
				{
					ImGui.Text("Nothing to view information for.");
				}
			}
			ImGui.End();

			ImGui.Begin("Hotbar", /*ImGuiWindowFlags.AlwaysAutoResize | */ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2 - ImGui.GetWindowWidth() / 2, gd.MainSwapchain.Framebuffer.Height / 5 * 4 - ImGui.GetWindowHeight() / 2), ImGuiCond.Always);
			ImGui.SetWindowSize(new Vector2(500, 50), ImGuiCond.Always);
			{
				ImGui.Columns(PipeTypes.Count);
				for (int i = 0; i < PipeTypes.Count; i++)
				{
					if (PipeTypes[i] is null)
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
						ImGui.TextColored(RgbaFloat.CornflowerBlue.ToVector4(), (PipeTypes[i].Name));
					}
					else
					{
						ImGui.TextDisabled(PipeTypes[i].Name);
					}

					ImGui.NextColumn();
				}
			}
			ImGui.End();
		}

		public void Style() // https://github.com/ocornut/imgui/issues/707
		{
			ImGuiIOPtr io = ImGui.GetIO();

			io.Fonts.Clear();
			io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.CurrentDirectory, "Assets", "External", "Fonts", "OpenSans", "static", "OpenSans", "OpenSans-Light.ttf"), 20/*16*/);
			io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.CurrentDirectory, "Assets", "External", "Fonts", "OpenSans", "static", "OpenSans", "OpenSans-Regular.ttf"), 20/*16*/);
			io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.CurrentDirectory, "Assets", "External", "Fonts", "OpenSans", "static", "OpenSans", "OpenSans-Light.ttf"), 32/*32*/);
			io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.CurrentDirectory, "Assets", "External", "Fonts", "OpenSans", "static", "OpenSans", "OpenSans-Regular.ttf"), 14/*11*/);
			io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.CurrentDirectory, "Assets", "External", "Fonts", "OpenSans", "static", "OpenSans", "OpenSans-Bold.ttf"), 14/*11*/);
			io.Fonts.Build();

			imGuiRenderer.RecreateFontDeviceTexture();

			ImGuiStylePtr style = ImGui.GetStyle();
			var colors = style.Colors;

			/// 0 = FLAT APPEARENCE
			/// 1 = MORE "3D" LOOK
			int is3D = 2;

			colors[(int)ImGuiCol.Text] = new(1.00f, 1.00f, 1.00f, 1.00f);
			colors[(int)ImGuiCol.TextDisabled] = new(0.70f, 0.70f, 0.70f, 1.00f);
			colors[(int)ImGuiCol.ChildBg] = new(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int)ImGuiCol.WindowBg] = new(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int)ImGuiCol.PopupBg] = new(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int)ImGuiCol.Border] = new(0.12f, 0.12f, 0.12f, 0.71f);
			colors[(int)ImGuiCol.BorderShadow] = new(1.00f, 1.00f, 1.00f, 0.06f);
			colors[(int)ImGuiCol.FrameBg] = new(0.42f, 0.42f, 0.42f, 0.54f);
			colors[(int)ImGuiCol.FrameBgHovered] = new(0.42f, 0.42f, 0.42f, 0.40f);
			colors[(int)ImGuiCol.FrameBgActive] = new(0.56f, 0.56f, 0.56f, 0.67f);
			colors[(int)ImGuiCol.TitleBg] = new(0.19f, 0.19f, 0.19f, 1.00f);
			colors[(int)ImGuiCol.TitleBgActive] = new(0.22f, 0.22f, 0.22f, 1.00f);
			colors[(int)ImGuiCol.TitleBgCollapsed] = new(0.17f, 0.17f, 0.17f, 0.90f);
			colors[(int)ImGuiCol.MenuBarBg] = new(0.335f, 0.335f, 0.335f, 1.000f);
			colors[(int)ImGuiCol.ScrollbarBg] = new(0.24f, 0.24f, 0.24f, 0.53f);
			colors[(int)ImGuiCol.ScrollbarGrab] = new(0.41f, 0.41f, 0.41f, 1.00f);
			colors[(int)ImGuiCol.ScrollbarGrabHovered] = new(0.52f, 0.52f, 0.52f, 1.00f);
			colors[(int)ImGuiCol.ScrollbarGrabActive] = new(0.76f, 0.76f, 0.76f, 1.00f);
			colors[(int)ImGuiCol.CheckMark] = new(0.65f, 0.65f, 0.65f, 1.00f);
			colors[(int)ImGuiCol.SliderGrab] = new(0.52f, 0.52f, 0.52f, 1.00f);
			colors[(int)ImGuiCol.SliderGrabActive] = new(0.64f, 0.64f, 0.64f, 1.00f);
			colors[(int)ImGuiCol.Button] = new(0.54f, 0.54f, 0.54f, 0.35f);
			colors[(int)ImGuiCol.ButtonHovered] = new(0.52f, 0.52f, 0.52f, 0.59f);
			colors[(int)ImGuiCol.ButtonActive] = new(0.76f, 0.76f, 0.76f, 1.00f);
			colors[(int)ImGuiCol.Header] = new(0.38f, 0.38f, 0.38f, 1.00f);
			colors[(int)ImGuiCol.HeaderHovered] = new(0.47f, 0.47f, 0.47f, 1.00f);
			colors[(int)ImGuiCol.HeaderActive] = new(0.76f, 0.76f, 0.76f, 0.77f);
			colors[(int)ImGuiCol.Separator] = new(0.000f, 0.000f, 0.000f, 0.137f);
			colors[(int)ImGuiCol.SeparatorHovered] = new(0.700f, 0.671f, 0.600f, 0.290f);
			colors[(int)ImGuiCol.SeparatorActive] = new(0.702f, 0.671f, 0.600f, 0.674f);
			colors[(int)ImGuiCol.ResizeGrip] = new(0.26f, 0.59f, 0.98f, 0.25f);
			colors[(int)ImGuiCol.ResizeGripHovered] = new(0.26f, 0.59f, 0.98f, 0.67f);
			colors[(int)ImGuiCol.ResizeGripActive] = new(0.26f, 0.59f, 0.98f, 0.95f);
			colors[(int)ImGuiCol.PlotLines] = new(0.61f, 0.61f, 0.61f, 1.00f);
			colors[(int)ImGuiCol.PlotLinesHovered] = new(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int)ImGuiCol.PlotHistogram] = new(0.90f, 0.70f, 0.00f, 1.00f);
			colors[(int)ImGuiCol.PlotHistogramHovered] = new(1.00f, 0.60f, 0.00f, 1.00f);
			colors[(int)ImGuiCol.TextSelectedBg] = new(0.73f, 0.73f, 0.73f, 0.35f);
			colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.80f, 0.80f, 0.80f, 0.35f);
			colors[(int)ImGuiCol.DragDropTarget] = new(1.00f, 1.00f, 0.00f, 0.90f);
			colors[(int)ImGuiCol.NavHighlight] = new(0.26f, 0.59f, 0.98f, 1.00f);
			colors[(int)ImGuiCol.NavWindowingHighlight] = new(1.00f, 1.00f, 1.00f, 0.70f);
			colors[(int)ImGuiCol.NavWindowingDimBg] = new(0.80f, 0.80f, 0.80f, 0.20f);

			style.PopupRounding = 3;

			style.WindowPadding = new(4, 4);
			style.FramePadding = new(6, 4);
			style.ItemSpacing = new(6, 2);

			style.ScrollbarSize = 18;

			style.WindowBorderSize = 1;
			style.ChildBorderSize = 1;
			style.PopupBorderSize = 1;
			style.FrameBorderSize = is3D;

			style.WindowRounding = 3;
			style.ChildRounding = 3;
			style.FrameRounding = 3;
			style.ScrollbarRounding = 2;
			style.GrabRounding = 3;
		}
	}
}
