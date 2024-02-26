using Space_Refinery_Engine;
using Space_Refinery_Engine.Audio;
using Space_Refinery_Utilities;
using Space_Refinery_Utilities.Units;
using System.Diagnostics;

namespace Tests
{
	[TestClass]
	public class InternalEnergy
	{
		[TestInitialize]
		public void Initialize()
		{
			ReferenceHandler = new();

			ReferenceHandler.EnterAllowEventualReferenceMode(false);
			{
				ResourceDeserialization.DeserializeIntoGlobalReferenceHandler(ReferenceHandler, new(), includeGameExtension: false);
			}
			ReferenceHandler.ExitAllowEventualReferenceMode();

			WaterChemical = ChemicalType.Water;

			WaterResource = WaterChemical.GetResourceTypeForPhase(ChemicalPhase.Liquid);

			WaterTemperatureEnergy = WaterChemical.SpecificHeatCapacity * WaterMass * WaterTemperature;
		}

		public SerializationReferenceHandler ReferenceHandler;
			   
		public ChemicalType WaterChemical;
			   
		public ResourceType WaterResource;
			   
		public MassUnit WaterMass = 1; // 1 kilogram of water
			   
		public TemperatureUnit WaterTemperature = 20 + 273.15; // 20℃
			   
		public EnergyUnit WaterTemperatureEnergy;

		[TestMethod]
		public void TemperatureToEnergy()
		{
			EnergyUnit internalTemperatureEnergy = ChemicalType.TemperatureToInternalEnergy(WaterResource, WaterTemperature, WaterMass);

			Assert.AreEqual<DecimalNumber>((DecimalNumber)internalTemperatureEnergy, (DecimalNumber)WaterTemperatureEnergy);
		}

		[TestMethod]
		public void EnergyToTemperature()
		{
			TemperatureUnit temperature = ChemicalType.InternalEnergyToTemperature(WaterResource, WaterTemperatureEnergy, WaterMass);

			Assert.AreEqual<DecimalNumber>((DecimalNumber)temperature, (DecimalNumber)WaterTemperature);
		}
	}
}