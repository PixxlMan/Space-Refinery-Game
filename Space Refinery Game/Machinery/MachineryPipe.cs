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

		public override void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, Connector transferingConnector)
		{
			lock (this)
			{
				source.TransferResource(ResourceContainers[(PipeConnector)transferingConnector], volume);
			}
		}

		protected override void DisplaceContents()
		{
			throw new NotImplementedException();
		}

		protected override void SetUp()
		{

		}

		protected override void Tick()
		{

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
