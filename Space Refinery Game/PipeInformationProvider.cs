using ImGuiNET;
using System.Globalization;

namespace Space_Refinery_Game
{
	public class PipeInformationProvider : IInformationProvider
	{
		public Pipe PipeStraght;

		public PipeInformationProvider(Pipe pipeStraght)
		{
			PipeStraght = pipeStraght;
		}

		public string Name => "Pipe";

		public void InformationUI()
		{
			ImGui.Spacing();
			
			ImGui.Text("Postition: " + PipeStraght.Renderable.Position.ToString("", CultureInfo.CurrentCulture));

			ImGui.Text("Orientation: " + PipeStraght.Renderable.Rotation.ToString());
		}
	}
}