using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace Space_Refinery_Game
{
	public interface IInformationProvider
	{
		public string Name { get; }

		public void InformationUI();
	}
}
