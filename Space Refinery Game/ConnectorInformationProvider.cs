using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace Space_Refinery_Game
{
	public class ConnectorInformationProvider : IInformationProvider
	{
		public string Name => "Connector";

		public void InformationUI()
		{
			ImGui.Text("Connector info");
		}
	}
}
