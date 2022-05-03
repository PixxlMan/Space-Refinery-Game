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

		public string Name => Pipe.PipeType.Name;

		public virtual void InformationUI()
		{
			ImGui.Spacing();
			
			ImGui.Text("Postition: " + Pipe.Renderable.Transform.Position.ToString("", CultureInfo.CurrentCulture));

			ImGui.Text("Orientation: " + Pipe.Renderable.Transform.Rotation.ToString());
		}
	}
}