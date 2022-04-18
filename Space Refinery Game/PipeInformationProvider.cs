using ImGuiNET;
using System.Globalization;

namespace Space_Refinery_Game
{
	public class PipeInformationProvider : IInformationProvider
	{
		public Pipe Pipe;

		public PipeInformationProvider(Pipe pipe)
		{
			Pipe = pipe;
		}

		public string Name => "Pipe";

		public virtual void InformationUI()
		{
			ImGui.Spacing();
			
			ImGui.Text("Postition: " + Pipe.Renderable.Position.ToString("", CultureInfo.CurrentCulture));

			ImGui.Text("Orientation: " + Pipe.Renderable.Rotation.ToString());
		}
	}
}