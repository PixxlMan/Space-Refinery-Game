using ImGuiNET;

namespace Space_Refinery_Game
{
	public class MachineryPipeInformationProvider : PipeInformationProvider
	{
		public MachineryPipeInformationProvider(MachineryPipe pipe) : base(pipe)
		{
		}

		public override void InformationUI()
		{
			base.InformationUI();

			var machineryPipe = (MachineryPipe)Pipe;

			foreach (var resourceContainer in machineryPipe.ResourceContainers.Values)
			{
				ImGui.Text(resourceContainer.ToString());
			}
		}
	}
}