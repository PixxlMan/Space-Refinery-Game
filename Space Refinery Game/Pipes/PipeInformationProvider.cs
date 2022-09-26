using ImGuiNET;
using System.Globalization;

namespace Space_Refinery_Game
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
				ImGui.Text("GUID: " + Pipe.SerializableReferenceGUID.ToString());

				ImGui.Text("Postition: " + Pipe.Transform.Position.ToString());

				ImGui.Text("Orientation: " + Pipe.Transform.Rotation.ToString());
			}
		}
	}
}