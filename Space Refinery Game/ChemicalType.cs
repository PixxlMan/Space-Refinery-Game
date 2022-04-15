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

		public FixedDecimalInt4 TemperatureForPlasmaPhaseChange;

		public GasType GasPhaseType;

		public FixedDecimalInt4 TemperatureForGasPhaseChange;

		public LiquidType LiquidPhaseType;

		public FixedDecimalInt4 TemperatureForLiquidPhaseChange;

		public SolidType SolidPhaseType;

		public void OnDeserialized()
		{
			PlasmaPhaseType.ChemicalType = this;
			GasPhaseType.ChemicalType = this;
			LiquidPhaseType.ChemicalType = this;
			SolidPhaseType.ChemicalType = this;
		}

		public ResourceType GetPhaseType(FixedDecimalInt4 temperature/*, FixedDecimalInt4 pressure*/)
		{
			if (temperature > TemperatureForPlasmaPhaseChange)
			{
				return PlasmaPhaseType;
			}
			else if (temperature > TemperatureForGasPhaseChange)
			{
				return GasPhaseType;
			}
			else if (temperature > TemperatureForLiquidPhaseChange)
			{
				return LiquidPhaseType;
			}
			else
			{
				return SolidPhaseType;
			}
		}

		public ChemicalPhase GetPhase(FixedDecimalInt4 temperature/*, FixedDecimalInt4 pressure*/)
		{
			if (temperature > TemperatureForPlasmaPhaseChange)
			{
				return ChemicalPhase.Plasma;
			}
			else if (temperature > TemperatureForGasPhaseChange)
			{
				return ChemicalPhase.Gas;
			}
			else if (temperature > TemperatureForLiquidPhaseChange)
			{
				return ChemicalPhase.Liquid;
			}
			else
			{
				return ChemicalPhase.Solid;
			}
		}

		public ResourceType GetPhaseType(ChemicalPhase chemicalPhase)
		{
			switch (chemicalPhase)
			{
				case ChemicalPhase.Solid:
					return SolidPhaseType;
				case ChemicalPhase.Liquid:
					return LiquidPhaseType;
				case ChemicalPhase.Gas:
					return GasPhaseType;
				case ChemicalPhase.Plasma:
					return PlasmaPhaseType;
				default:
					throw null;
			}
		}

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

		public static ChemicalType[] LoadChemicalTypes(string directory)
		{
			List<ChemicalType> chemicalTypes = new();

			foreach (var filePath in Directory.GetFiles(directory))
			{
				using var stream = File.OpenRead(filePath);

				chemicalTypes.Add(Deserialize(filePath));
			}

			return chemicalTypes.ToArray();
		}
	}
}
