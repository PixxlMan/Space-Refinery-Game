using ImGuiNET;

namespace Space_Refinery_Engine
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
		}
	}
}