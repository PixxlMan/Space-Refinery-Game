using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace Space_Refinery_Game
{
	public class PipeConnectorInformationProvider : IInformationProvider
	{
		public PipeConnector PipeConnector;

		public PipeConnectorInformationProvider(PipeConnector pipe)
		{
			PipeConnector = pipe;
		}

		public string Name => "Connector";

		public void InformationUI()
		{
			ImGui.Text($"Vacant side: {PipeConnector.VacantSide}");

			ImGui.Text($"Connector position: {PipeConnector.Transform.Position.ToString()}");
			ImGui.Text($"Connector rotation: {PipeConnector.Transform.Rotation}");

			ImGui.Text($"Pipe A: {(PipeConnector.Pipes.pipeA is null ? "None" : "Connected")}");
			ImGui.Text($"Pipe B: {(PipeConnector.Pipes.pipeB is null ? "None" : "Connected")}");
		}
	}
}
