using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public PipeType SelectedPipeType => PipeTypes[Selection];

		public int Selection;

		public void ChangeSelection(int selectionDelta)
		{
			Selection += selectionDelta;
			 
			while (Selection >= PipeTypes.Count || Selection < 0)
			{
				if (Selection < 0)
				{
					Selection += PipeTypes.Count;
				}
				else if (Selection >= PipeTypes.Count)
				{
					Selection -= PipeTypes.Count;
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

		public void DoUI()
		{
			ImGui.Begin("Center", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2, gd.MainSwapchain.Framebuffer.Height / 2), ImGuiCond.Always);
			{
				ImGui.Bullet();
			}
			ImGui.End();

			ImGui.Begin("Information panel", ImGuiWindowFlags.AlwaysAutoResize /*| ImGuiWindowFlags.NoBackground */| ImGuiWindowFlags.NoDecoration);
			ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 4 * 3, gd.MainSwapchain.Framebuffer.Height / 2), ImGuiCond.Always);
			{
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
						if (Selection == i)
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

					if (Selection == i)
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
