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

		public static readonly FixedDecimalLong8 ProcessingContainerVolume = 1;

		public ResourceContainer ProcessingContainer = new(ProcessingContainerVolume);

		public static readonly FixedDecimalLong8 InOutPipeVolume = (FixedDecimalLong8).4;

		public static readonly FixedDecimalLong8 ElectrolyzationRate = (FixedDecimalLong8).0005; // m3/s

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

		static readonly FixedDecimalLong8 hydrogenPart = ((FixedDecimalLong8)1 / (FixedDecimalLong8)3) * (FixedDecimalLong8)2;

		static readonly FixedDecimalLong8 oxygenPart = ((FixedDecimalLong8)1 / (FixedDecimalLong8)3);

		bool blocked;

		public ResourceContainer OutputBuffer = new(FixedDecimalLong8.MaxValue);

		protected override void DisplaceContents()
		{
			//throw new NotImplementedException();
		}

		protected override void Tick()
		{
			base.Tick();

			lock (this)
			{
				if (blocked)
				{
					if (OutputBuffer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Hydrogen"].GasPhaseType))
					{
						ResourceUnit bufferedHydrogen = OutputBuffer.GetResourceUnitForResourceType(MainGame.ChemicalTypesDictionary["Hydrogen"].GasPhaseType);
						if (HydrogenOutput.Volume + bufferedHydrogen.Volume < HydrogenOutput.MaxVolume)
						{
							HydrogenOutput.AddResource(OutputBuffer.ExtractResource(bufferedHydrogen.ResourceType, bufferedHydrogen.Volume));
						}
					}

					if (OutputBuffer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Water"].GasPhaseType))
					{
						ResourceUnit bufferedOxygen = OutputBuffer.GetResourceUnitForResourceType(MainGame.ChemicalTypesDictionary["Oxygen"].GasPhaseType);
						if (OxygenOutput.Volume + bufferedOxygen.Volume < OxygenOutput.MaxVolume)
						{
							OxygenOutput.AddResource(OutputBuffer.ExtractResource(bufferedOxygen.ResourceType, bufferedOxygen.Volume));
						}
					}

					/*if (!OutputBuffer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Hydrogen"].GasPhaseType) || !OutputBuffer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Water"].GasPhaseType))
					{
						blocked = false;
					}*/
				}

				if (Activated/* && !blocked*/)
				{
					WaterInput.TransferResource(ProcessingContainer, FixedDecimalLong8.Clamp(WaterInput.Volume * WaterInput.Fullness * (FixedDecimalLong8)Time.TickInterval, 0, WaterInput.Volume));

					if (ProcessingContainer.ContainsResourceType(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType))
					{
						FixedDecimalLong8 electrolyzedVolume = FixedDecimalLong8.Min(ElectrolyzationRate * (FixedDecimalLong8)Time.TickInterval, ProcessingContainer.GetResourceUnitForResourceType(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType).Volume); // cache, just reset on Tick pace change?

						var electrolyzedUnit = ProcessingContainer.ExtractResource(MainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType, electrolyzedVolume);

						ResourceUnit hydrogenUnit = new(MainGame.ChemicalTypesDictionary["Hydrogen"].GasPhaseType, electrolyzedUnit.Mass * hydrogenPart, electrolyzedUnit.InternalEnergy * hydrogenPart, electrolyzedUnit.Pressure * hydrogenPart);

						ResourceUnit oxygenUnit = new(MainGame.ChemicalTypesDictionary["Oxygen"].GasPhaseType, electrolyzedUnit.Mass * oxygenPart, electrolyzedUnit.InternalEnergy * oxygenPart, electrolyzedUnit.Pressure * oxygenPart);

						if (HydrogenOutput.Volume + hydrogenUnit.Volume > HydrogenOutput.MaxVolume  || OxygenOutput.Volume + oxygenUnit.Volume > OxygenOutput.MaxVolume)
						{
							OutputBuffer.AddResource(hydrogenUnit);
							OutputBuffer.AddResource(oxygenUnit);

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
