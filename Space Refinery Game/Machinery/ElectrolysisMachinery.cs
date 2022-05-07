using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

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

			if (blocked)
			{
				ImGui.TextColored(RgbaFloat.Red.ToVector4(), "Blocked");
			}

			if (!Activated || blocked)
			{
				UIFunctions.PushDisabled();
			}

			if (!Activated || blocked)
			{
				UIFunctions.PopDisabled();
			}
		}

		static readonly FixedDecimalInt4 hydrogenPart = ((FixedDecimalInt4)1 / (FixedDecimalInt4)3) * (FixedDecimalInt4)2;

		static readonly FixedDecimalInt4 oxygenPart = ((FixedDecimalInt4)1 / (FixedDecimalInt4)3);

		bool blocked;

		protected override void Tick()
		{
			base.Tick();

			lock (this)
			{
				if (Activated)
				{
					WaterInput.TransferResource(ProcessingContainer, FixedDecimalLong8.Clamp(WaterInput.Volume * WaterInput.Fullness * (FixedDecimalLong8)Time.TickInterval, 0, WaterInput.Volume));

					if (ProcessingContainer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType))
					{
						FixedDecimalLong8 electrolyzedVolume = FixedDecimalLong8.Min(ElectrolyzationRate * (FixedDecimalLong8)Time.TickInterval, ProcessingContainer.GetResourceUnitForResourceType(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType).Volume); // cache, just reset on Tick pace change?

						var electrolyzedUnit = ProcessingContainer.ExtractResource(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType, electrolyzedVolume);

						ResourceUnit hydrogenUnit = new(MainGame.ChemicalTypesDictionary["Hydrogen"].GasPhaseType, electrolyzedUnit.Mass * hydrogenPart, electrolyzedUnit.InternalEnergy * hydrogenPart, electrolyzedUnit.Pressure * hydrogenPart);

						ResourceUnit oxygenUnit = new(MainGame.ChemicalTypesDictionary["Oxygen"].GasPhaseType, electrolyzedUnit.Mass * oxygenPart, electrolyzedUnit.InternalEnergy * oxygenPart, electrolyzedUnit.Pressure * oxygenPart);

						if (HydrogenOutput.Volume + hydrogenUnit.Volume > (FixedDecimalLong8)HydrogenOutput.MaxVolume  || OxygenOutput.Volume + oxygenUnit.Volume > (FixedDecimalLong8)OxygenOutput.MaxVolume)
						{
							blocked = true;
							return;
						}
						else
						{
							blocked = false;
						}

						HydrogenOutput.AddResource(hydrogenUnit); // use molar or atom mass later? this isnt really correct since dihydrogen and dioxygen weigh differently...

						OxygenOutput.AddResource(oxygenUnit);

						//ElectricityInput.ConsumeElectricity();
					}
				}
			}
		}
	}
}
