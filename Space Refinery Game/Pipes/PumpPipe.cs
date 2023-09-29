using ImGuiNET;
using System.Xml;
using static Space_Refinery_Game.DecimalNumber;

namespace Space_Refinery_Game
{
	public sealed class PumpPipe : Pipe
	{
		public bool DirectionAToB;

		public ResourceContainer ContainerA, ContainerB;

		public ResourceContainer Transferer => DirectionAToB ? ContainerA : ContainerB;

		public ResourceContainer Recipient => DirectionAToB ? ContainerB : ContainerA;

		public PipeConnector ConnectorA, ConnectorB;

		/// <summary>
		/// [m³/s]
		/// </summary>
		public static readonly Rate<VolumeUnit> MaxFlowRate = 1;

		public PumpPipe()
		{
			informationProvider = new PumpPipeInformationProvider(this);
		}

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			if (ConnectorA == pipeConnector)
			{
				return ContainerA;
			}
			else if(ConnectorB == pipeConnector)
			{
				return ContainerB;
			}

			throw new ArgumentException("Connector is not connected to this pipe.", nameof(pipeConnector));
		}

		public override void TransferResourceFromConnector(ResourceContainer source, VolumeUnit volume, PipeConnector transferingConnector)
		{
			lock (SyncRoot)
			{
				source.TransferResourceByVolume(GetResourceContainerForConnector(transferingConnector), volume);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			lock (SyncRoot)
			{
				ContainerA = new(PipeType.PipeProperties.FlowableVolume * (Portion<VolumeUnit>)0.5);
				ContainerB = new(PipeType.PipeProperties.FlowableVolume * (Portion<VolumeUnit>)0.5);

				ConnectorA = NamedConnectors["A"];
				ConnectorB = NamedConnectors["B"];
			}
		}

		public override void Tick()
		{
			base.Tick();

			lock (SyncRoot)
			{
				Transferer.TransferResourceByVolume(
					Recipient,
					(VolumeUnit)Min(
						Min(
							(DecimalNumber)(MaxFlowRate * Time.TickInterval)
							, (DecimalNumber)(Transferer.Volume * (Portion<VolumeUnit>)0.5))
						, (DecimalNumber)(Recipient.FreeVolume * (Portion<VolumeUnit>)0.5))
					);
			}
		}

		protected override void Interacted()
		{
			base.Interacted();

			UI.EnterMenu(DoMenu, "Pump");
		}

		protected override void DisplaceContents()
		{
			base.DisplaceContents();

			if (ConnectorA.Vacant && ConnectorB.Vacant)
			{
				return;
			}

			if (ConnectorA.Vacant)
			{
				ConnectorB.TransferResource(this, ContainerA, ContainerA.Volume);
				ConnectorB.TransferResource(this, ContainerB, ContainerB.Volume);
			}
			else if (ConnectorB.Vacant)
			{
				ConnectorA.TransferResource(this, ContainerA, ContainerA.Volume);
				ConnectorA.TransferResource(this, ContainerB, ContainerB.Volume);
			}
			else
			{
				ConnectorA.TransferResource(this, ContainerA, ContainerA.Volume);
				ConnectorB.TransferResource(this, ContainerB, ContainerB.Volume);
			}
		}

		private void DoMenu()
		{
			if (ImGui.Button("Direction toggle"))
			{
				DirectionAToB = !DirectionAToB;
			}

			ImGui.Text(DirectionAToB ? "A->B" : "B->A");
		}

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.Serialize(DirectionAToB, nameof(DirectionAToB));

			ContainerA.Serialize(writer);
			ContainerB.Serialize(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			DirectionAToB = reader.DeserializeBoolean(nameof(DirectionAToB));

			ContainerA = ResourceContainer.Deserialize(reader);
			ContainerB = ResourceContainer.Deserialize(reader);
		}
	}
}