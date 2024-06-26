﻿using Veldrid;
using ImGuiNET;
using FixedPrecision;
using Space_Refinery_Engine.Renderer;

namespace Space_Refinery_Engine
{
	public sealed partial class UI : IRenderable
	{
		private readonly ImGuiRenderer imGuiRenderer;

		private GraphicsDevice gd;

		private GameData gameData;

		private List<IEntityType?> hotbarItems = new();

		public IEntityType? SelectedEntityType { get { lock (syncRoot) return hotbarItems[EntitySelection]; } }

		public int EntitySelection;

		public event Action<IEntityType?> SelectedEntityTypeChanged;

		public bool Paused;

		public int RotationIndex;

		public FixedDecimalLong8 RotationSnapping => 45 * FixedDecimalLong8.DegreesToRadians;
		
		public FixedDecimalLong8 RotationSnapped => gameData.UI.RotationIndex * RotationSnapping;

		public event Action<FixedDecimalLong8> SelectedEntityRotated;

		public int? ConnectorSelection;

		public event Action<int?> SelectedEntityConnectorChanged;

		public event Action<bool> PauseStateChanged;

		private readonly object syncRoot = new();

		private DecimalNumber hotbarFading = 1;

		private int width;
		private int height;

		private Vector2FixedDecimalInt4 Size => new(width, height);

		ImDrawListPtr drawList;

		public void Restore()
		{
			gameData.GraphicsWorld.AddRenderable(this, 10_000);

			gameData.GraphicsWorld.WindowResized += WindowResized;
		}

		public void ChangeEntitySelection(int selectionDelta)
		{
			lock (syncRoot)
			{
				EntitySelection += selectionDelta;

				while (EntitySelection >= hotbarItems.Count || EntitySelection < 0)
				{
					if (EntitySelection < 0)
					{
						EntitySelection += hotbarItems.Count;
					}
					else if (EntitySelection >= hotbarItems.Count)
					{
						EntitySelection -= hotbarItems.Count;
					}
				}

				hotbarFading = 1;

				SelectedEntityTypeChanged?.Invoke(SelectedEntityType);
			}
		}

		public void SetEntitySelection(int selection)
		{
			lock (syncRoot)
			{
				if (selection > hotbarItems.Count - 1 || selection < 0)
				{
					return;
				}

				EntitySelection = selection;

				hotbarFading = 1;

				SelectedEntityTypeChanged?.Invoke(SelectedEntityType);
			}
		}

		private UI(GameData gameData)
		{
			this.gameData = gameData;
			gd = gameData.GraphicsWorld.GraphicsDevice;

			imGuiRenderer = new(gd, gameData.GraphicsWorld.RenderingOutputDescription, width, height);
			imGuiRenderer.CreateDeviceResources(gd, gameData.GraphicsWorld.RenderingOutputDescription);
		}

		private void WindowResized(int width, int height)
		{
			lock (syncRoot)
			{
				imGuiRenderer.WindowResized(width, height);
				imGuiRenderer.DestroyDeviceObjects();
				imGuiRenderer.CreateDeviceResources(gd, gameData.GraphicsWorld.RenderingOutputDescription);

				this.width = width;
				this.height = height;
			}
		}

		private Action doMenu;

		public bool InMenu { get; private set; }
		public string MenuTitle { get; private set; }

		public void EnterMenu(Action doMenu, string title)
		{
			lock (syncRoot)
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
		}

		public static UI CreateAndAdd(GameData gameData)
		{
			Logging.LogScopeStart("Creating UI");

			ImGui.CreateContext();

			UI ui = new(gameData);

			ui.hotbarItems.Add(null); // Add empty slot

			ui.Style();

			ui.Restore();

			Logging.LogScopeEnd();

			return ui;
		}

		public void AddDrawCommands(CommandList cl, FixedDecimalLong8 deltaTime)
		{
			lock (syncRoot)
			{
				var snapshot = InputTracker.CreateInputTrackerCloneSnapshot();
				try
				{
					imGuiRenderer.Update(1, snapshot);
				}
				catch (ArgumentOutOfRangeException)
				{
					Logging.LogError("That weird argument out of range exception again... Oh well! Skipping the UI this frame.");
					return; // The error appears to be correlated with AccessViolationExceptions
							// in ImGui (especially when dealing with windows).
							// So it might be safest to skip drawing the ui when the error occurs.
				}

				BeforeUI();

				DoUI(deltaTime);

				PostUI(cl);
			}
		}

		private void BeforeUI()
		{
			drawList = ImGui.GetBackgroundDrawList();
		}

		private void PostUI(CommandList cl)
		{
			imGuiRenderer.WindowResized((int)width, (int)height);

			imGuiRenderer.Render(gd, cl);
		}

		public void Update()
		{
			if (!InMenu && !Paused)
			{
				if (InputTracker.GetKeyDown(Key.R) && InputTracker.GetKey(Key.ShiftLeft))
				{
					Interlocked.Decrement(ref RotationIndex);

					SelectedEntityRotated?.Invoke(RotationSnapped);
				}
				else if (InputTracker.GetKeyDown(Key.R))
				{
					Interlocked.Increment(ref RotationIndex);

					SelectedEntityRotated?.Invoke(RotationSnapped);
				}

				if (InputTracker.ScrollWheelDelta != 0)
				{
					ChangeEntitySelection(-(int)InputTracker.ScrollWheelDelta);
				}

				if (InputTracker.GetKeyDown(Key.Number1))
				{
					SetEntitySelection(0);
				}
				else if(InputTracker.GetKeyDown(Key.Number2))
				{
					SetEntitySelection(1);
				}
				else if(InputTracker.GetKeyDown(Key.Number3))
				{
					SetEntitySelection(2);
				}
				else if(InputTracker.GetKeyDown(Key.Number4))
				{
					SetEntitySelection(3);
				}
				else if(InputTracker.GetKeyDown(Key.Number5))
				{
					SetEntitySelection(4);
				}
				else if(InputTracker.GetKeyDown(Key.Number6))
				{
					SetEntitySelection(5);
				}
				else if(InputTracker.GetKeyDown(Key.Number7))
				{
					SetEntitySelection(6);
				}
				else if(InputTracker.GetKeyDown(Key.Number8))
				{
					SetEntitySelection(7);
				}
				else if(InputTracker.GetKeyDown(Key.Number9))
				{
					SetEntitySelection(8);
				}
				else if(InputTracker.GetKeyDown(Key.Number0))
				{
					SetEntitySelection(9);
				}
			}

			if (InMenu && InputTracker.CaptureKeyDown(Key.F) && !Paused)
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
			lock (syncRoot)
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
		}

		public void Pause()
		{
			lock (syncRoot)
			{
				if (Paused)
				{
					return;
				}

				Paused = true;

				PauseStateChanged?.Invoke(true);
			}
		}

		public void Unpause()
		{
			lock (syncRoot)
			{
				if (!Paused)
				{
					return;
				}

				Paused = false;

				inSettings = false;

				PauseStateChanged?.Invoke(false);
			}
		}

		public void DoUI(FixedDecimalLong8 deltaTime)
		{
			lock (syncRoot)
			{
				if (!Paused)
				{
					DoGameRunningUI(deltaTime);
				}
				else if (Paused)
				{
					DoPauseMenuUI(deltaTime);
				}
			}
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

		public void SetTargetedPhysicsObject(PhysicsObject lookedAtPhysicsObject)
		{
			gameData.UI.CurrentlyLookedAtPhysicsObject = lookedAtPhysicsObject;
		}

		public void SetSelectedInformationProvider(IInformationProvider informationProvider)
		{
			gameData.UI.CurrentlySelectedInformationProvider = informationProvider;
		}

		public void ClearTargetedPhysicsObject()
		{
			gameData.UI.CurrentlyLookedAtPhysicsObject = null;
		}

		public void ClearSelectedInformationProvider()
		{
			gameData.UI.CurrentlySelectedInformationProvider = null;
		}
	}
}
