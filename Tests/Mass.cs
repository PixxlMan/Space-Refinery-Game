using Space_Refinery_Engine;
using Space_Refinery_Engine.Audio;
using Space_Refinery_Utilities;
using Space_Refinery_Utilities.Units;
using System.Diagnostics;

namespace Tests
{
	[TestClass]
	public class Mass
	{
		[TestInitialize]
		public void Initialize()
		{
			WaterChemical = ChemicalType.Water;

			WaterResource = WaterChemical.GetResourceTypeForPhase(ChemicalPhase.Liquid);
		}

		public SerializationReferenceHandler ReferenceHandler;
			   
		public ChemicalType WaterChemical;
			   
		public ResourceType WaterResource;
			   
		public MassUnit WaterMass = 1; // 1 kilogram of water

		public MolesUnit WaterMoles = 1 / 0.0180153; // 1 kg of water divided by water's kg/mol

		[TestMethod]
		public void MassToMoles()
		{
			MolesUnit moles = ChemicalType.MassToMoles(WaterChemical, WaterMass);

			Assert.AreEqual<DecimalNumber>((DecimalNumber)WaterMoles, (DecimalNumber)moles);
		}

		[TestMethod]
		public void MolesToMass()
		{
			MassUnit mass = ChemicalType.MolesToMass(WaterChemical, WaterMoles);

			Assert.IsTrue(WaterMass.Difference(mass) <= 0.001);
		}
	}
}