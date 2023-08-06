using ImGuiNET;
using System.Collections.Concurrent;

namespace Space_Refinery_Game
{
	public abstract class MachineryPipe : Pipe
	{
		protected MachineryPipe()
		{
			informationProvider = new MachineryPipeInformationProvider(this);
		}

		public ConcurrentDictionary<PipeConnector, ResourceContainer> ConnectorToResourceContainers = new();

		public ConcurrentDictionary<string, ResourceContainer> ResourceContainers = new();

		public bool Activated;

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			lock (SyncRoot)
			{
				return ConnectorToResourceContainers[pipeConnector];
			}
		}

		public override void TransferResourceFromConnector(ResourceContainer source, VolumeUnit volume, PipeConnector transferingConnector)
		{
			lock (SyncRoot)
			{
				source.TransferResourceByVolume(ConnectorToResourceContainers[transferingConnector], volume);
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
