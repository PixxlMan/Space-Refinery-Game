using Space_Refinery_Engine;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class MachineryCombustionChamber : MachineryPipe
	{
		private MachineryCombustionChamber() : base()
		{ }

		public ResourceContainer OxygenInput;

		public ResourceContainer FuelInput;

		public ResourceContainer ProductOutput;

		public static readonly VolumeUnit ReactionContainerVolume = 1;

		public ResourceContainer ReactionContainer = new(ReactionContainerVolume);

		public static readonly VolumeUnit InOutPipeVolume = (VolumeUnit)(DecimalNumber).4;

		public static readonly DecimalNumber sparkEnergy = 20 * DecimalNumber.Micro; // A 20 µJ spark is the minimum spark required to start an oxyhydrogen combustion.

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
					case "OxygenInput":
						OxygenInput = resourceContainer;
						break;
					case "FuelInput":
						FuelInput = resourceContainer;
						break;
					case "ProductOutput":
						ProductOutput = resourceContainer;
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

		public override void Tick()
		{
			base.Tick();

			lock (SyncRoot)
			{
				if (Activated)
				{
					OxygenInput.TransferResourceByVolume(ReactionContainer, ChemicalType.Oxygen.GasPhaseType,
						UnitsMath.Clamp(
							OxygenInput.NonCompressableVolume * OxygenInput.Fullness * Time.TickInterval,
							0,
							UnitsMath.Min(ReactionContainer.NonCompressableUnoccupiedVolume, OxygenInput.VolumeOf(ChemicalType.Oxygen.GasPhaseType))));
					
					FuelInput.TransferResourceByVolume(ReactionContainer,
						UnitsMath.Clamp(
							FuelInput.NonCompressableVolume * FuelInput.Fullness * Time.TickInterval,
							0,
							UnitsMath.Min(ReactionContainer.NonCompressableUnoccupiedVolume, FuelInput.NonCompressableVolume)));

					ReactionContainer.AddReactionFactor(new Spark(sparkEnergy));

					ReactionContainer.Tick(Time.TickInterval);

					OxygenInput.Tick(Time.TickInterval);

					FuelInput.Tick(Time.TickInterval);

					ProductOutput.Tick(Time.TickInterval);

					ReactionContainer.TransferResourceByVolume(
						ProductOutput,
						UnitsMath.Min(
							ReactionContainer.VolumeOf(ChemicalType.Water.LiquidPhaseType),
							UnitsMath.Max(
								ReactionContainer.Fullness - ProductOutput.Fullness, 0)
							* Time.TickInterval * ReactionContainer.NonCompressableVolume)
						);

					//ElectricityInput.ConsumeElectricity();
				}
			}
		}

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.Serialize(Activated, nameof(Activated));

			ReactionContainer.Serialize(writer);
			OxygenInput.Serialize(writer);
			ProductOutput.Serialize(writer);
			FuelInput.Serialize(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			Activated = reader.DeserializeBoolean(nameof(Activated));

			ReactionContainer = ResourceContainer.Deserialize(reader);
			OxygenInput = ResourceContainer.Deserialize(reader);
			ProductOutput = ResourceContainer.Deserialize(reader);
			FuelInput = ResourceContainer.Deserialize(reader);
		}
	}
}
