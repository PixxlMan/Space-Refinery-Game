using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	[Serializable]
	public class ChemicalType : IJsonOnDeserialized
	{
		public string ChemicalName;

		public PlasmaType PlasmaPhaseType;

		public GasType GasPhaseType;

		public LiquidType LiquidPhaseType;

		public SolidType SolidPhaseType;

		public void OnDeserialized()
		{
			PlasmaPhaseType.ChemicalType = this;
			GasPhaseType.ChemicalType = this;
			LiquidPhaseType.ChemicalType = this;
			SolidPhaseType.ChemicalType = this;
		}

		/*public ResourceType GetPhase(FixedDecimalInt4 temperature, FixedDecimalInt4 pressure)
		{
		
		}*/
	}
}
