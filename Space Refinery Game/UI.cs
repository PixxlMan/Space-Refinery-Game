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

			if (CurrentlySelectedInformationProvider is not null)
			{
				ImGui.Begin("Information panel", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
				ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 4 * 3, gd.MainSwapchain.Framebuffer.Height / 2), ImGuiCond.Always);
				{
					ImGui.Text($"Information for: {CurrentlySelectedInformationProvider.Name}");
					CurrentlySelectedInformationProvider.InformationUI();
				}
				ImGui.End();
			}
		}
	}
}
