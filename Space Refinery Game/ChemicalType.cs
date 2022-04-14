using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

		public void Serialize(string path)
		{
			using var stream = File.OpenWrite(path);

			JsonSerializer.Serialize(stream, this, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true, ReadCommentHandling = JsonCommentHandling.Skip });
		}

		public static ChemicalType Deserialize(string path)
		{
			using var stream = File.OpenRead(path);

			return JsonSerializer.Deserialize<ChemicalType>(stream, new JsonSerializerOptions() { IncludeFields = true, ReadCommentHandling = JsonCommentHandling.Skip });
		}
	}
}
