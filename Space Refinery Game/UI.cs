using System;
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
	public class UI : IRenderable
	{
		public IInformationProvider CurrentlySelectedInformationProvider;

		private ImGuiRenderer imGuiRenderer;

		private GraphicsDevice gd;

		public List<PipeType> PipeTypes = new();

		public PipeType SelectedPipeType => PipeTypes[EntitySelection];

		public int EntitySelection;

		public bool Paused;

		public FixedDecimalInt4 Rotation;

		public event Action<bool> PauseStateChanged;

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

		public static UI Create(GraphicsWorld graphWorld)
		{
			ImGui.CreateContext();

			UI ui = new(graphWorld.GraphicsDevice);

			graphWorld.AddRenderable(ui, 1);

			ui.PipeTypes.AddRange(PipeType.GetAllPipeTypes(graphWorld));

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

			if (InputTracker.GetKey(Key.R) && InputTracker.GetKey(Key.ShiftLeft))
			{
				Rotation -= 10 * Time.UpdateInterval;
			}
			else if (InputTracker.GetKey(Key.R))
			{
				Rotation += 10 * Time.UpdateInterval;
			}

			if (InputTracker.FrameSnapshot.WheelDelta != 0)
			{
				ChangeEntitySelection(-(int)InputTracker.FrameSnapshot.WheelDelta);
			}

			if (InputTracker.GetKeyDown(Key.Escape))
			{
				TogglePause();
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
			if (ImGui.Begin("Debug Settings"))
			{
				foreach (var debugSetting in MainGame.DebugSettings.DebugSettingsDictionary.Values)
				{
					debugSetting.DrawUIElement();
				}

				ImGui.End();
			}
		}

		private Vector2 pauseMenuSize => new Vector2(gd.MainSwapchain.Framebuffer.Width / 3, (gd.MainSwapchain.Framebuffer.Height / 10) * 8);

		private void DoPauseMenuUI()
		{
			ImGui.Begin("Pause menu", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2 - pauseMenuSize.X / 2, gd.MainSwapchain.Framebuffer.Height / 2 - pauseMenuSize.Y / 2), ImGuiCond.Always);
			ImGui.SetWindowSize(pauseMenuSize, ImGuiCond.Always);
			{
				if (ImGui.Button("Resume"))
				{
					Unpause();
				}

				if (ImGui.Button("Exit game"))
				{
					Environment.Exit(69);
				}
			}
			ImGui.End();

			DoDebugSettingsUI();
		}

		private void DoGameRunningUI()
		{
			ImGui.Begin("Staus", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
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

			ImGui.Begin("Information panel", ImGuiWindowFlags.AlwaysAutoResize /*| ImGuiWindowFlags.NoBackground */| ImGuiWindowFlags.NoDecoration);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 4 * 3, gd.MainSwapchain.Framebuffer.Height / 2), ImGuiCond.Always);
			{
				ImGui.Text("Connector: " + ConnectorSelection);
				ImGui.Text("Entity: " + EntitySelection);
				ImGui.Text("Rotation: " + Rotation);

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

			ImGui.Begin("Hotbar", /*ImGuiWindowFlags.AlwaysAutoResize | */ImGuiWindowFlags.NoDecoration);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2, gd.MainSwapchain.Framebuffer.Height / 5 * 4), ImGuiCond.Always);
			ImGui.SetWindowSize(new Vector2(500, 50), ImGuiCond.Always);
			{
				ImGui.Columns(PipeTypes.Count);
				for (int i = 0; i < PipeTypes.Count; i++)
				{
					if (PipeTypes[i] is null)
					{
						if (EntitySelection == i)
						{
							ImGui.TextColored(RgbaFloat.Blue.ToVector4(), "None");
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
						ImGui.TextColored(RgbaFloat.Blue.ToVector4(), (PipeTypes[i].Name));
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
	}
}
