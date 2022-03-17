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

namespace Space_Refinery_Game
{
	public class UI : IRenderable
	{
		private ImGuiRenderer imGuiRenderer;

		private GraphicsDevice gd;

		public UI(GraphicsDevice gd)
		{
			ImGui.CreateContext();

			imGuiRenderer = new(gd, gd.MainSwapchain.Framebuffer.OutputDescription, (int)gd.MainSwapchain.Framebuffer.Width, (int)gd.MainSwapchain.Framebuffer.Height);

			imGuiRenderer.CreateDeviceResources(gd, gd.MainSwapchain.Framebuffer.OutputDescription);

			this.gd = gd;
		}

		public void AddDrawCommands(CommandList cl)
		{
			imGuiRenderer.Update(1, InputTracker.FrameSnapshot);

			ImGui.Begin("Test", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration);
				ImGui.SetWindowPos(new Vector2(gd.MainSwapchain.Framebuffer.Width / 2, gd.MainSwapchain.Framebuffer.Height / 2));

				ImGui.LabelText("Test", "");
			ImGui.End();

			imGuiRenderer.WindowResized((int)gd.MainSwapchain.Framebuffer.Width, (int)gd.MainSwapchain.Framebuffer.Height);

			imGuiRenderer.Render(gd, cl);
		}
	}
}
