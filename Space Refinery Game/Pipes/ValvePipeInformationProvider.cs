using ImGuiNET;

namespace Space_Refinery_Game
{
	public sealed class ValvePipeInformationProvider : PipeInformationProvider
	{
		public ValvePipeInformationProvider(ValvePipe pipe) : base(pipe)
		{
		}

		public override void InformationUI()
		{
			base.InformationUI();

			var valvePipe = (ValvePipe)Pipe;

			ImGui.Text($"Limiter: {valvePipe.Limiter}");

			foreach (var resourceContainer in valvePipe.ResourceContainers.Values)
			{
				ImGui.Text(resourceContainer.ToString());
			}
		}
	}
}