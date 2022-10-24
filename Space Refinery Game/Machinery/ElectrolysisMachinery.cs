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

		public static DecimalNumber AmperageDrawMax => 100;

		public static DecimalNumber MaxElectricalEnergyPerSecond => AmperageDrawMax * Electricity.Voltage;

		protected override void SetUp()
		{
			base.SetUp();

			foreach (var nameConnectorPair in NamedConnectors)
			{
				ResourceContainer resourceContainer = new(InOutPipeVolume);

				ConnectorToResourceContainers.AddUnique(nameConnectorPair.Value, resourceContainer);

				ResourceContainers.AddUnique($"{nameConnectorPair.Key} container", resourceContainer);

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

			ResourceContainers.AddUnique("Reaction container", ReactionContainer);
		}

		protected override void DoMenu()
		{
			base.DoMenu();
		}

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

					electrolysisReaction.Tick(Time.TickInterval, ReactionContainer, new ReactionFactor[1] { new ElectricalCurrent(MaxElectricalEnergyPerSecond * (DecimalNumber)Time.TickInterval) });

					if (ReactionContainer.ContainsResourceType(ChemicalType.Oxygen.GasPhaseType))
					{
						OxygenOutput.FillResource(ReactionContainer.ExtractResourceByVolume(ChemicalType.Oxygen.GasPhaseType, ReactionContainer.GetResourceUnitForResourceType(ChemicalType.Oxygen.GasPhaseType).Volume), out var rest);

						ReactionContainer.AddResource(rest);
					}

					if (ReactionContainer.ContainsResourceType(ChemicalType.Hydrogen.GasPhaseType))
					{
						HydrogenOutput.FillResource(ReactionContainer.ExtractResourceByVolume(ChemicalType.Hydrogen.GasPhaseType, ReactionContainer.GetResourceUnitForResourceType(ChemicalType.Hydrogen.GasPhaseType).Volume), out var rest);

						ReactionContainer.AddResource(rest);
					}

					//ElectricityInput.ConsumeElectricity();
				}
			}
		}

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.Serialize(Activated, nameof(Activated));

			ReactionContainer.Serialize(writer);
			WaterInput.Serialize(writer);
			OxygenOutput.Serialize(writer);
			HydrogenOutput.Serialize(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			Activated = reader.DeserializeBoolean(nameof(Activated));

			ReactionContainer = ResourceContainer.Deserialize(reader);
			WaterInput = ResourceContainer.Deserialize(reader);
			OxygenOutput = ResourceContainer.Deserialize(reader);
			HydrogenOutput = ResourceContainer.Deserialize(reader);
		}
	}
}
