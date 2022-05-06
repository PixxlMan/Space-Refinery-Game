using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class ElectrolysisMachinery : MachineryPipe
	{
		protected ElectrolysisMachinery() : base()
		{ }

		public ResourceContainer WaterInput;

		public ResourceContainer HydrogenOutput;

		public ResourceContainer OxygenOutput;

		public static readonly FixedDecimalInt4 ProcessingContainerVolume = 1;

		public ResourceContainer ProcessingContainer = new(ProcessingContainerVolume);

		public static readonly FixedDecimalInt4 InOutPipeVolume = (FixedDecimalInt4).2;

		public static readonly FixedDecimalLong8 ElectrolyzationRate = (FixedDecimalLong8).05; // m3/s

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

			if (!Activated)
			{
				UIFunctions.PushDisabled();
			}

			if (!Activated)
			{
				UIFunctions.PopDisabled();
			}
		}

		protected override void Tick()
		{
			lock (this)
			{
				base.Tick();

				if (Activated)
				{
					WaterInput.TransferResource(ProcessingContainer, FixedDecimalLong8.Clamp(WaterInput.Volume * WaterInput.Fullness * (FixedDecimalLong8)Time.TickInterval, 0, WaterInput.Volume));

					if (ProcessingContainer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType))
					{
						FixedDecimalLong8 electrolyzedVolume = FixedDecimalLong8.Min(ElectrolyzationRate * (FixedDecimalLong8)Time.TickInterval, ProcessingContainer.GetResourceUnitForResourceType(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType).Volume); // cache, just reset on Tick pace change?

						var electrolyzedUnit = ProcessingContainer.ExtractResource(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType, electrolyzedVolume);

						HydrogenOutput.AddResource(ResourceUnit.Part(electrolyzedUnit, (electrolyzedUnit.Mass / 3) * 2)); // use molar or atom mass later? this isnt really correct since dihydrogen and dioxygen weigh differently...

						OxygenOutput.AddResource(ResourceUnit.Part(electrolyzedUnit, electrolyzedUnit.Mass / 3));

						//ElectricityInput.ConsumeElectricity();
					}
				}
			}
		}
	}
}
