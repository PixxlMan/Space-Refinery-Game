﻿using FixedPrecision;
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
	public sealed class MachineryCombustionChamber : MachineryPipe
	{
		private MachineryCombustionChamber() : base()
		{ }

		public ResourceContainer OxygenInput;

		public ResourceContainer FuelInput;

		public ResourceContainer ProductOutput;

		public static readonly DecimalNumber ReactionContainerVolume = 1;

		public ResourceContainer ReactionContainer = new(ReactionContainerVolume);

		public static readonly DecimalNumber InOutPipeVolume = (DecimalNumber).4;

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

			lock (this)
			{
				if (Activated)
				{
					OxygenInput.TransferResourceByVolume(ReactionContainer, ChemicalType.Oxygen.LiquidPhaseType,
						DecimalNumber.Clamp(
							OxygenInput.Volume * OxygenInput.Fullness * (DecimalNumber)Time.TickInterval,
							0,
							ReactionContainer.FreeVolume));
					
					FuelInput.TransferResourceByVolume(ReactionContainer,
						DecimalNumber.Clamp(
							OxygenInput.Volume * OxygenInput.Fullness * (DecimalNumber)Time.TickInterval,
							0,
							ReactionContainer.FreeVolume));

					ReactionContainer.Tick(Time.TickInterval, new ReactionFactor[1] { new Spark(20 * DecimalNumber.Micro /*20 µJ*/) });

					ReactionContainer.TransferResourceByVolume(ProductOutput, DecimalNumber.Min(ReactionContainer.VolumeOf(ChemicalType.Oxygen.GasPhaseType), DecimalNumber.Max(ProductOutput.Fullness - ReactionContainer.Fullness, 0) * (DecimalNumber)Time.TickInterval));

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
