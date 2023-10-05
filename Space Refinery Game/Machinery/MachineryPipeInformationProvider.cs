using ImGuiNET;
using Space_Refinery_Engine;

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

			foreach (var (name, resourceContainer) in machineryPipe.ResourceContainers)
			{
				ImGui.Text($"{nameof(ResourceContainer)}: {name}:");
				resourceContainer.DoUIInspectorReadonly();
				ImGui.Separator();
			}
		}
	}
}