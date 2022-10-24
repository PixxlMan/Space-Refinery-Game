using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Veldrid;

namespace Space_Refinery_Game
{
	public sealed class ElectrolysisMachinery : MachineryPipe
	{
		protected ElectrolysisMachinery() : base()
		{ }

		public ResourceContainer WaterInput;

		public ResourceContainer HydrogenOutput;

		public ResourceContainer OxygenOutput;

		public static readonly DecimalNumber ProcessingContainerVolume = 1;

		public ResourceContainer ReactionContainer = new(ProcessingContainerVolume);

		public static readonly DecimalNumber InOutPipeVolume = (DecimalNumber).4;

		public static readonly DecimalNumber ElectrolyzationRate = (DecimalNumber).0005; // [m³/s]

		public static DecimalNumber AmperageDrawMax => 100;

		public static DecimalNumber MaxElectricalEnergyPerSecond => AmperageDrawMax * Electricity.Voltage;

		protected override void SetUp()
		{
			base.SetUp();

			foreach (var nameConnectorPair in NamedConnectors)
			{
				ResourceContainer resourceContainer = new(InOutPipeVolume);

				ResourceContainers.Add(nameConnectorPair.Value, resourceContainer);

				switch (nameConnectorPair.Key)
				{
					case "WaterInput":
						WaterInput = resourceContainer;
						break;
					case "HydrogenOutput":
						HydrogenOutput = resourceContainer;
						break;
					case "OxygenOutput":
						OxygenOutput = resourceContainer;
						break;
				}
			}
		}

		protected override void DoMenu()
		{
			base.DoMenu();
		}

		public ResourceContainer OutputBuffer = new(DecimalNumber.MaxValue);

		protected override void DisplaceContents()
		{
			//throw new NotImplementedException();
		}

		ElectrolysisReactionType electrolysisReaction = (ElectrolysisReactionType)ReactionType.ReactionTypes[0];

		public override void Tick()
		{
			base.Tick();

			lock (this)
			{
				if (Activated)
				{
					WaterInput.TransferResource(ReactionContainer, DecimalNumber.Clamp(WaterInput.Volume * WaterInput.Fullness * (DecimalNumber)Time.TickInterval, 0, WaterInput.Volume));

					electrolysisReaction.ProvideElectricalPower(MaxElectricalEnergyPerSecond);

					electrolysisReaction.Tick(Time.TickInterval, ReactionContainer);

					if (ReactionContainer.ContainsResourceType(ChemicalType.Oxygen.GasPhaseType))
					{
						OxygenOutput.AddResource(ReactionContainer.ExtractResourceByVolume(ChemicalType.Oxygen.GasPhaseType, ReactionContainer.GetResourceUnitForResourceType(ChemicalType.Oxygen.GasPhaseType).Volume));
					}

					if (ReactionContainer.ContainsResourceType(ChemicalType.Hydrogen.GasPhaseType))
					{
						HydrogenOutput.AddResource(ReactionContainer.ExtractResourceByVolume(ChemicalType.Hydrogen.GasPhaseType, ReactionContainer.GetResourceUnitForResourceType(ChemicalType.Hydrogen.GasPhaseType).Volume));
					}

					//ElectricityInput.ConsumeElectricity();
				}
			}
		}

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.Serialize(Activated, nameof(Activated));

			WaterInput.Serialize(writer);
			OxygenOutput.Serialize(writer);
			HydrogenOutput.Serialize(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			Activated = reader.DeserializeBoolean(nameof(Activated));

			WaterInput = ResourceContainer.Deserialize(reader);
			OxygenOutput = ResourceContainer.Deserialize(reader);
			HydrogenOutput = ResourceContainer.Deserialize(reader);
		}
	}
}
