using ImGuiNET;
using System.Globalization;

namespace Space_Refinery_Game
{
	public class PipeInformationProvider : IInformationProvider
	{
		public Pipe Pipe;

		public PipeInformationProvider(Pipe pipeStraght)
		{
			Pipe = pipeStraght;
		}

		public string Name => "Pipe";

		public void InformationUI()
		{
			ImGui.Spacing();
			
			ImGui.Text("Postition: " + Pipe.Renderable.Position.ToString("", CultureInfo.CurrentCulture));

			ImGui.Text("Orientation: " + Pipe.Renderable.Rotation.ToString());

			ImGui.Text($"Fullness: {Pipe.Fullness}");

			ImGui.Text($"Contents: {Pipe.ResourceContainer.ToString()}");
		}
	}
}