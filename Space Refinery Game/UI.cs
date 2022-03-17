using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using ImGuiNET;
using FixedPrecision;

namespace Space_Refinery_Game
{
	public class UI
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

		public void DrawUI(CommandList cl, FixedDecimalInt4 deltaTime)
		{
			imGuiRenderer.Update(deltaTime.ToFloat(), InputTracker.FrameSnapshot);

			ImGui.Begin("Test");
			if (ImGui.Button("1233"))
			{

			}
			ImGui.Text("Bouunjjoorr");
			ImGui.End();

			ImGui.ShowMetricsWindow();

			imGuiRenderer.WindowResized((int)gd.MainSwapchain.Framebuffer.Width, (int)gd.MainSwapchain.Framebuffer.Height);

			imGuiRenderer.Render(gd, cl);
		}
	}
}
