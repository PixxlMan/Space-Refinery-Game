using ImGuiNET;
using Space_Refinery_Engine;

namespace Space_Refinery_Game
{
	public sealed class OrdinaryPipeInformationProvider : PipeInformationProvider
	{
		public OrdinaryPipeInformationProvider(OrdinaryPipe pipe) : base(pipe)
		{
		}

		public override void InformationUI()
		{
			base.InformationUI();

			var ordinaryPipe = (OrdinaryPipe)Pipe;

			ImGui.Text($"{nameof(ResourceContainer)}:");

			ordinaryPipe.ResourceContainer.DoUIInspectorReadonly();

			//ImGui.Text("Statistics:");
		}
	}
}