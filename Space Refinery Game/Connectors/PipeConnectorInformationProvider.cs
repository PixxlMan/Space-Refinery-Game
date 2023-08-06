using ImGuiNET;

namespace Space_Refinery_Game
{
	public sealed class PipeConnectorInformationProvider : IInformationProvider
	{
		public PipeConnector PipeConnector;

		public PipeConnectorInformationProvider(PipeConnector pipe)
		{
			PipeConnector = pipe;
		}

		public string Name => "Connector";

		public void InformationUI()
		{
			if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show debug information in information provider"))
			{
				ImGui.Text($"Vacant side: {PipeConnector.VacantSide}");

				ImGui.Text($"Connector position: {PipeConnector.Transform.Position.ToString("", null)}");
				ImGui.Text($"Connector rotation: {PipeConnector.Transform.Rotation}");

				ImGui.Text($"Pipe A: {(PipeConnector.Pipes.pipeA is null ? "None" : "Connected")}");
				ImGui.Text($"Pipe B: {(PipeConnector.Pipes.pipeB is null ? "None" : "Connected")}");

				ImGui.Text("GUID: " + PipeConnector.SerializableReference.ToString());

				ImGui.Text("Postition: " + PipeConnector.Transform.Position.ToString());

				ImGui.Text("Orientation: " + PipeConnector.Transform.Rotation.ToString());
			}
		}
	}
}
