using CsvHelper;
using CsvHelper.Configuration.Attributes;
using ImGuiNET;
using Space_Refinery_Engine;
using System.Formats.Asn1;
using System.Globalization;
using System.Xml;
using Veldrid;
using static Space_Refinery_Utilities.DecimalNumber;

namespace Space_Refinery_Game;

public sealed class ExperimentPipe : MachineryPipe
{
	private ExperimentPipe()
	{
		informationProvider = new ExperimentPipeInformationProvider(this);
	}

	public ResourceContainer ResourceContainer;

	public override void Tick()
	{
		lock (SyncRoot)
		{
#if DEBUG
			DebugStopPoints.TickStopPoint(SerializableReference);
#endif

			ResourceContainer.Tick(Time.TickInterval);

			if (Activated)
			{
				if (ResourceContainer.VolumeCapacity - 0.0001 <= 0)
				{
					Activated = false;
					return;
				}

				RecordedPressureAndTemp.Add(new((decimal)(DecimalNumber)ResourceContainer.Pressure, (decimal)(DecimalNumber)ResourceContainer.AverageTemperature));

				ResourceContainer.VolumeCapacity -= 0.0001;
			}
		}
	}

	float energyToAdd = 10_000;

	protected override void DoMenu()
	{
		lock (SyncRoot)
		{
			if (ImGui.Button("Insert 1 kg air (0.77 m³ volume)"))
			{
				TemperatureUnit temperature = Calculations.CelciusToTemperature(20);

				MassUnit totalMass = 1;

				var nitrogen = ChemicalType.GetChemicalType("Nitrogen");
				var nitrogenMass = (78.084 / 100) * totalMass;
				MolesUnit nitrogenPart = ChemicalType.MassToMoles(nitrogen, nitrogenMass);

				var oxygen = ChemicalType.Oxygen;
				var oxygenMass = (20.948 / 100) * totalMass;
				MolesUnit oxygenPart = ChemicalType.MassToMoles(oxygen, oxygenMass);

				var water = ChemicalType.Water;
				var waterMass = (1 / 100) * totalMass;
				MolesUnit waterPart = ChemicalType.MassToMoles(water, waterMass);

				var argon = ChemicalType.GetChemicalType("Argon");
				var argonMass = (0.934 / 100) * totalMass;
				MolesUnit argonPart = ChemicalType.MassToMoles(argon, argonMass);

				ResourceContainer.AddResource(new(nitrogen.GasPhaseType, nitrogenPart, ChemicalType.TemperatureToInternalEnergy(nitrogen.GasPhaseType, temperature, nitrogenMass)));
				ResourceContainer.AddResource(new(oxygen.GasPhaseType, oxygenPart, ChemicalType.TemperatureToInternalEnergy(oxygen.GasPhaseType, temperature, oxygenMass)));
				ResourceContainer.AddResource(new(water.GasPhaseType, waterPart, ChemicalType.TemperatureToInternalEnergy(water.GasPhaseType, temperature, waterMass)));
				ResourceContainer.AddResource(new(argon.GasPhaseType, argonPart, ChemicalType.TemperatureToInternalEnergy(argon.GasPhaseType, temperature, argonMass)));
			}

			ImGui.SliderFloat("Heat energy to add (kJ)", ref energyToAdd, 1_000, 100_000);
			if (ImGui.Button("Add heat"))
			{
				EnergyUnit energy = (EnergyUnit)(DN)energyToAdd;
				EnergyUnit timeAdjustedEnergy = energy * Time.UpdateInterval;

				foreach (var unitData in ResourceContainer.EnumerateResources())
				{
					ResourceContainer.AddResource(new(unitData.ResourceType, 0, energy * (Portion<EnergyUnit>)(DN)(unitData.Mass / ResourceContainer.Mass)));
				}
			}

			ImGui.Separator();

			base.DoMenu();

			ImGui.Text($"{RecordedPressureAndTemp.Count} entries recorded");

			if (ImGui.Button("Save"))
			{
				SaveData();
			}

			if (ImGui.Button("Clear"))
			{
				ClearData();
			}
		}
	}

	protected override void Interacted()
	{
		base.Interacted();
	}

	public record struct PressureTempRecord(decimal pressure, decimal temp)
	{
		[Name("Pressure")]
		public decimal Pressure = pressure;

		[Name("Temperature")]
		public decimal Temperature = temp;
	}

	public List<PressureTempRecord> RecordedPressureAndTemp = new();

	public void SaveData()
	{
		lock (SyncRoot)
		{
			using CsvWriter writer = new(new StreamWriter(File.OpenWrite("R:\\pressure_temperature.csv")), CultureInfo.InvariantCulture, false);

			writer.WriteRecords(RecordedPressureAndTemp);

			writer.Flush();

			writer.Dispose();
		}
	}

	public void ClearData()
	{
		lock (SyncRoot)
		{
			RecordedPressureAndTemp.Clear();
		}
	}

	public override void TransferResourceFromConnector(ResourceContainer source, VolumeUnit volume, PipeConnector _)
	{
		lock (SyncRoot)
		{
			ResourceContainer.TransferResourceByVolume(source, volume);
		}
	}

	protected override void DisplaceContents()
	{
		lock (SyncRoot)
		{
			List<PipeConnector> connectedConnectors = new();
			foreach (var connector in Connectors)
			{
				if (!connector.Vacant)
					connectedConnectors.Add(connector);
			}

			if (connectedConnectors.Count == 0)
			{
				return;
			}

			var volumePerConnector = (VolumeUnit)((DecimalNumber)ResourceContainer.NonCompressableVolume / connectedConnectors.Count);

			foreach (var connectedConnector in connectedConnectors)
			{
				connectedConnector.TransferResource(this, ResourceContainer, volumePerConnector);
			}
		}
	}

	protected override void SetUp()
	{
		lock (SyncRoot)
		{
			ResourceContainer ??= new(PipeType.PipeProperties.FlowableVolume);
		}
	}

	public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
	{
		return ResourceContainer;
	}

	public override void SerializeState(XmlWriter writer)
	{
		base.SerializeState(writer);

		ResourceContainer.Serialize(writer);
	}

	public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		base.DeserializeState(reader, serializationData, referenceHandler);

		ResourceContainer = ResourceContainer.Deserialize(reader);
	}
}

public sealed class ExperimentPipeInformationProvider : PipeInformationProvider
{
	private ExperimentPipe experimentPipe;

	public ExperimentPipeInformationProvider(Pipe pipe) : base(pipe)
	{
		experimentPipe = (ExperimentPipe)pipe;
	}

	public override void InformationUI()
	{
		base.InformationUI();

		experimentPipe.ResourceContainer.DoUIInspectorReadonly();
	}
}