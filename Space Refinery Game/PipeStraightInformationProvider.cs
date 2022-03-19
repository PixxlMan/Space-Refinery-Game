using ImGuiNET;
using System.Globalization;

namespace Space_Refinery_Game
{
	public class PipeStraightInformationProvider : IInformationProvider
	{
		public PipeStraight PipeStraght;

		public PipeStraightInformationProvider(PipeStraight pipeStraght)
		{
			PipeStraght = pipeStraght;
		}

		public string Name => "Pipe Straight";

		public void InformationUI()
		{
			ImGui.Spacing();
			
			ImGui.Text("Postition: " + PipeStraght.Renderable.Position.ToString("", CultureInfo.CurrentCulture));
		}
	}
}