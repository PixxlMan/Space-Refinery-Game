using ImGuiNET;

namespace Space_Refinery_Engine
{
	public class PipeInformationProvider : IInformationProvider
	{
		public Pipe Pipe;

		public PipeInformationProvider(Pipe pipe)
		{
			Pipe = pipe;
		}

		public string Name => Pipe.PipeType.Name;

		public virtual void InformationUI()
		{
			ImGui.Spacing();

			if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show debug information in information provider"))
			{
				ImGui.Text("GUID: " + Pipe.SerializableReference.ToString());

				ImGui.Text("Postition: " + Pipe.Transform.Position.ToString());

				ImGui.Text("Orientation: " + Pipe.Transform.Rotation.ToString());

				ImGui.Text("Pipe Type: " + Pipe.PipeType.Name);

				if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show rendering debug information in information provider"))
				{
					ImGui.Text("Batch renderer: " + Pipe.PipeType.BatchRenderable.Name);

					ImGui.Text("Rendering index: " + Pipe.PipeType.BatchRenderable.DebugGetRenderableIndex(Pipe));
				}
			}
		}
	}
}