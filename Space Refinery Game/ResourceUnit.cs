using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public struct ResourceUnit
	{
		public ResourceType ResourceType;

		public ChemicalType ChemicalType => ResourceType.ChemicalType;

		public FixedDecimalInt4 Mass; // kg

		public FixedDecimalLong8 Volume => (FixedDecimalLong8)Mass * ResourceType.Density; // m3

		public FixedDecimalInt4 InternalEnergy; // j

		public FixedDecimalInt4 Temperature => InternalEnergy / (Mass * ResourceType.SpecificHeatCapacity); // k

		public FixedDecimalInt4 Pressure; // kPa or kg/m3

		public FixedDecimalInt4 Enthalpy => InternalEnergy + Pressure * (FixedDecimalInt4)Volume; // https://www.omnicalculator.com/physics/enthalpy
	}
}
