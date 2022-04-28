using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class SpaceDockPipe : Pipe
	{
		private SpaceDockPipe()
		{
			informationProvider = new SpaceDockPipeInformationProvider(this);
		}

		public List<ResourceUnit> Orders = new();

		public ResourceContainer ResourceContainer;

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			return ResourceContainer;
		}

		public override void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, Connector transferingConnector)
		{
			lock (this)
			{
				ResourceContainer.TransferResource(source, volume);
			}
		}

		protected override void DisplaceContents()
		{
			lock (this)
			{
				List<PipeConnector> connectedConnectors = new();
				foreach (var connector in Connectors)
				{
					if (!connector.Vacant)
						connectedConnectors.Add(connector);
				}

				var volumePerConnector = ResourceContainer.Volume / connectedConnectors.Count;

				foreach (var connectedConnector in connectedConnectors)
				{
					connectedConnector.TransferResource(this, ResourceContainer, volumePerConnector);
				}
			}
		}

		protected override void SetUp()
		{
			lock (this)
			{
				ResourceContainer = new(PipeType.PipeProperties.FlowableVolume);
			}
		}

		protected override void Tick()
		{

		}

		protected override void Interacted()
		{
			UI.EnterMenu(DoMenu, "Space Dock");
		}

		int selection = -1;
		ResourceUnit newResourceUnit;
		private void DoMenu()
		{
			lock (this)
			{
				UIFunctions.DoSelector(MainGame.ChemicalTypes, ref selection, out bool hasSelection, out ChemicalType selected);

				if (hasSelection)
				{
					ResourceUnit.DoCreation(selected, ref newResourceUnit);

					if (ImGui.Button("Add"))
					{
						Orders.Add(newResourceUnit);
					}
				}

				if (ImGui.Button("Place order"))
				{
					foreach (var order in Orders)
					{
						ResourceContainer.AddResource(order);
					}
				}
			}
		}
	}
}
