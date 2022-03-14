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

		private Framebuffer framebuffer;

		private GraphicsDevice gd;

		public UI(GraphicsDevice gd, Framebuffer framebuffer)
		{
			ImGui.CreateContext();

			imGuiRenderer = new(gd, framebuffer.OutputDescription, (int)framebuffer.Width, (int)framebuffer.Height);

			imGuiRenderer.CreateDeviceResources(gd, framebuffer.OutputDescription);

			this.framebuffer = framebuffer;
		}

		public void DrawUI(CommandList cl)
		{
			ImGui.Begin("Test");
			ImGui.End();

			imGuiRenderer.WindowResized((int)framebuffer.Width, (int)framebuffer.Height);

			imGuiRenderer.Render(gd, cl);
		}

		public void Update(FixedDecimalInt4 deltaTime)
		{
			imGuiRenderer.Update(deltaTime.ToFloat(), InputTracker.FrameSnapshot);
		}
	}
}
