using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public abstract class MachineryPipe : Pipe
	{
		protected MachineryPipe()
		{
			informationProvider = new MachineryPipeInformationProvider(this);
		}

		public Dictionary<PipeConnector, ResourceContainer> ResourceContainers = new();

		public bool Activated;

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			lock (this)
			{
				return ResourceContainers[pipeConnector];
			}
		}

		public override void TransferResourceFromConnector(ResourceContainer source, DecimalNumber volume, PipeConnector transferingConnector)
		{
			lock (this)
			{
				source.TransferResource(ResourceContainers[transferingConnector], volume);
			}
		}

		protected override void SetUp()
		{

		}

		public override void Tick()
		{
			base.Tick();
		}

		protected override void Interacted()
		{
			UI.EnterMenu(DoMenu, "Machinery");
		}

		protected virtual void DoMenu()
		{
			ImGui.Checkbox("Powered", ref Activated);
		}
	}
}
