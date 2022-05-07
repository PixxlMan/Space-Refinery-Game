using FixedPrecision;
using ImGuiNET;

namespace Space_Refinery_Game
{
	public sealed class PumpPipe : Pipe
	{
		public bool DirectionAToB;

		public ResourceContainer ContainerA, ContainerB;

		public ResourceContainer Transferer => DirectionAToB ? ContainerA : ContainerB;

		public ResourceContainer Recipient => DirectionAToB ? ContainerB : ContainerA;

		public FixedDecimalLong8 MaxFlowRate = 1; // m3/s

		public PumpPipe()
		{
			informationProvider = new PumpPipeInformationProvider(this);
		}

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			if (NamedConnectors["A"] == pipeConnector)
			{
				return ContainerA;
			}
			else if(NamedConnectors["B"] == pipeConnector)
			{
				return ContainerB;
			}

			throw new ArgumentException("Connector is not connected to this pipe.", nameof(pipeConnector));
		}

		public override void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, PipeConnector transferingConnector)
		{
			lock (this)
			{
				source.TransferResource(GetResourceContainerForConnector(transferingConnector), volume);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			lock (this)
			{
				ContainerA = new(PipeType.PipeProperties.FlowableVolume / 2);
				ContainerB = new(PipeType.PipeProperties.FlowableVolume / 2);
			}
		}

		protected override void Tick()
		{
			base.Tick();

			lock (this)
			{
				var transferVolume = FixedDecimalLong8.Min(Transferer.Volume * (FixedDecimalLong8)Time.TickInterval, FixedDecimalLong8.Min(MaxFlowRate * (FixedDecimalLong8)Time.TickInterval, Recipient.MaxVolume - Recipient.Volume - (FixedDecimalLong8)0.0001));

				Transferer.TransferResource(Recipient, transferVolume);
			}
		}

		protected override void Interacted()
		{
			base.Interacted();

			UI.EnterMenu(DoMenu, "Pump");
		}

		protected void DoMenu()
		{
			if (ImGui.Button("Direction toggle"))
			{
				DirectionAToB = !DirectionAToB;
			}

			ImGui.Text(DirectionAToB ? "A->B" : "B->A");
		}
	}
}