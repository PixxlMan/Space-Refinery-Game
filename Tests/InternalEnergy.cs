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
			WaterChemical = ChemicalType.Water;

			WaterResource = WaterChemical.GetResourceTypeForPhase(ChemicalPhase.Liquid);

			WaterTemperatureEnergy = WaterChemical.SpecificHeatCapacity * WaterMass * WaterTemperature;
		}

		public ChemicalType WaterChemical;
			   
		public ResourceType WaterResource;
			   
		public MassUnit WaterMass = 1; // 1 kilogram of water
			   
		public TemperatureUnit WaterTemperature = Calculations.CelciusToTemperature(20); // 20℃
			   
		public EnergyUnit WaterTemperatureEnergy;

		[TestMethod]
		public void TemperatureToEnergy()
		{
			EnergyUnit internalTemperatureEnergy = ChemicalType.TemperatureToInternalEnergy(WaterResource, WaterTemperature, WaterMass);

			Assert.AreEqual<DecimalNumber>((DecimalNumber)WaterTemperatureEnergy, (DecimalNumber)internalTemperatureEnergy);
		}

		[TestMethod]
		public void EnergyToTemperature()
		{
			TemperatureUnit temperature = ChemicalType.InternalEnergyToTemperature(WaterResource, WaterTemperatureEnergy, WaterMass);

			Assert.AreEqual<DecimalNumber>((DecimalNumber)WaterTemperature, (DecimalNumber)temperature);
		}

		[TestMethod]
		public void InternalEnergySymmetry()
		{
			EnergyUnit internalTemperatureEnergy = ChemicalType.TemperatureToInternalEnergy(WaterResource, WaterTemperature, WaterMass);

			TemperatureUnit temperature = ChemicalType.InternalEnergyToTemperature(WaterResource, internalTemperatureEnergy, WaterMass);

			Assert.AreEqual<DecimalNumber>((DecimalNumber)WaterTemperature, (DecimalNumber)temperature);
		}
	}
}