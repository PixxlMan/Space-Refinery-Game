using Space_Refinery_Engine;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class ElectrolysisMachinery : MachineryPipe
	{
		private ElectrolysisMachinery() : base()
		{ }

		public ResourceContainer WaterInput;

		public ResourceContainer HydrogenOutput;

		public ResourceContainer OxygenOutput;

		public static readonly VolumeUnit ReactionContainerVolume = 1;

		public ResourceContainer ReactionContainer = new(ReactionContainerVolume);

		public static readonly VolumeUnit InOutPipeVolume = (VolumeUnit)(DecimalNumber).4;

		public static AmperageUnit AmperageDrawMax => 100;

		/// <summary>
		/// [J/s]
		/// </summary>
		public static Rate<EnergyUnit> MaxElectricalEnergyPerSecond => AmperageDrawMax * Electricity.Voltage;

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

			lock (SyncRoot)
			{
				if (Activated)
				{
					WaterInput.TransferResourceByVolume(ReactionContainer, ChemicalType.Water.LiquidPhaseType,
						(VolumeUnit)DecimalNumber.Clamp(
							(DecimalNumber)(WaterInput.Volume * WaterInput.Fullness) * (DecimalNumber)Time.TickInterval,
							0,
							(DecimalNumber)ReactionContainer.FreeVolume));

					electrolysisReaction.Tick(Time.TickInterval, ReactionContainer, new ReactionFactor[1] { new ElectricalCurrent(MaxElectricalEnergyPerSecond * Time.TickInterval) }.ToLookup((rF) => rF.GetType()), null);
					// cache and don't regenerate reaction factors every time?
					ReactionContainer.TransferResourceByVolume(OxygenOutput, ChemicalType.Oxygen.GasPhaseType, (VolumeUnit)DecimalNumber.Min((DecimalNumber)ReactionContainer.VolumeOf(ChemicalType.Oxygen.GasPhaseType), (DecimalNumber)(OxygenOutput.FreeVolume * (Portion<VolumeUnit>)0.8)));

					ReactionContainer.TransferResourceByVolume(HydrogenOutput, ChemicalType.Hydrogen.GasPhaseType, (VolumeUnit)DecimalNumber.Min((DecimalNumber)ReactionContainer.VolumeOf(ChemicalType.Hydrogen.GasPhaseType), (DecimalNumber)(HydrogenOutput.FreeVolume * (Portion<VolumeUnit>)0.8)));

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
