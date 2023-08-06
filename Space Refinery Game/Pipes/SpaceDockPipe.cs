using ImGuiNET;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class SpaceDockPipe : Pipe
	{
		private SpaceDockPipe()
		{
			informationProvider = new SpaceDockPipeInformationProvider(this);
		}

		public List<ResourceUnitData> Orders = new();

		public ResourceContainer ResourceContainer;

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			return ResourceContainer;
		}

		public override void TransferResourceFromConnector(ResourceContainer source, VolumeUnit volume, PipeConnector transferringConnector)
		{
			lock (SyncRoot)
			{
				ResourceContainer.TransferResourceByVolume(source, volume);
			}
		}

		protected override void DisplaceContents()
		{
			lock (SyncRoot)
			{
				List<PipeConnector> connectedConnectors = new();
				foreach (var connector in Connectors)
				{
					if (!connector.Vacant)
						connectedConnectors.Add(connector);
				}

				if (connectedConnectors.Count == 0)
				{
					return;
				}

				var volumePerConnector = (VolumeUnit)((DecimalNumber)ResourceContainer.Volume / (DecimalNumber)connectedConnectors.Count);

				foreach (var connectedConnector in connectedConnectors)
				{
					connectedConnector.TransferResource(this, ResourceContainer, volumePerConnector);
				}
			}
		}

		protected override void SetUp()
		{
			lock (SyncRoot)
			{
				ResourceContainer = new(PipeType.PipeProperties.FlowableVolume);
			}
		}

		public override void Tick()
		{
			base.Tick();
		}

		protected override void Interacted()
		{
			UI.EnterMenu(DoMenu, "Space Dock");
		}

		int selection = -1;
		ResourceUnitData newResourceUnit;
		private Guid guid = Guid.NewGuid();
		private void DoMenu()
		{
			lock (SyncRoot)
			{
				if (ImGui.CollapsingHeader("Resource selection"))
				{
					UIFunctions.DoSelector(ChemicalType.ChemicalTypes.ToArray(), guid, ref selection, out bool hasSelection, out ChemicalType selected);

					if (hasSelection)
					{
						ResourceUnitData.DoCreation(selected, ref newResourceUnit);

						if (ImGui.Button("Add"))
						{
							Orders.Add(newResourceUnit);
						}
					}
				}

				if (ImGui.CollapsingHeader("Selected orderables"))
				{
					UIFunctions.DoListManipulation(Orders, guid, false);
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

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			ResourceContainer.Serialize(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			ResourceContainer = ResourceContainer.Deserialize(reader);
		}
	}
}
