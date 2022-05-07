using ImGuiNET;

namespace Space_Refinery_Game
{
	public class PumpPipeInformationProvider : PipeInformationProvider
	{
		public PumpPipeInformationProvider(PumpPipe pipe) : base(pipe)
		{
		}

		public override void InformationUI()
		{
			base.InformationUI();

			var pumpPipe = (PumpPipe)Pipe;

			ImGui.Text(pumpPipe.ContainerA.ToString());

			ImGui.Text(pumpPipe.ContainerB.ToString());
		}
	}
}