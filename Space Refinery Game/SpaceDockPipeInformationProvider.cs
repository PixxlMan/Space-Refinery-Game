using ImGuiNET;

namespace Space_Refinery_Game
{
	public class SpaceDockPipeInformationProvider : PipeInformationProvider
	{
		public SpaceDockPipeInformationProvider(SpaceDockPipe pipe) : base(pipe)
		{
		}

		public override void InformationUI()
		{
			base.InformationUI();

			var spaceDockPipe = (SpaceDockPipe)Pipe;

			ImGui.Text($"{nameof(ResourceContainer)}: {spaceDockPipe.ResourceContainer.ToString()}");
		}
	}
}