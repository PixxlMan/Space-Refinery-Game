using FixedPrecision;
using ImGuiNET;
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
	public sealed class ChemicalType : IJsonOnDeserialized, IUIInspectable
	{
		public string ChemicalName;

		public GasType GasPhaseType;

		public LiquidType LiquidPhaseType;

		public SolidType SolidPhaseType;

		public DecimalNumber EnthalpyOfVaporization;

		public DecimalNumber EnthalpyOfFusion;

		public void OnDeserialized()
		{
			GasPhaseType.ChemicalType = this;
			LiquidPhaseType.ChemicalType = this;
			SolidPhaseType.ChemicalType = this;
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

		public void DoUIInspectorReadonly()
		{
			UIFunctions.BeginSub();
			{
				ImGui.Text($"Chemical name: {ChemicalName}");

				if (ImGui.CollapsingHeader("Resource phases"))
				{
					ImGui.Indent();
					{
						if (ImGui.CollapsingHeader("Gas phase type"))
						{
							GasPhaseType.DoUIInspectorReadonly();
						}
						if (ImGui.CollapsingHeader("Liquid phase type"))
						{
							LiquidPhaseType.DoUIInspectorReadonly();
						}
						if (ImGui.CollapsingHeader("Solid phase type"))
						{
							SolidPhaseType.DoUIInspectorReadonly();
						}
					}
					ImGui.Unindent();
				}

				ImGui.Text($"Enthalpy of vaporization: {EnthalpyOfVaporization}");
				ImGui.Text($"Enthalpy of fusion: {EnthalpyOfFusion}");
			}
			UIFunctions.EndSub();
		}

		public IUIInspectable DoUIInspectorEditable()
		{
			throw new NotImplementedException();
		}

		public ResourceType GetResourceTypeForPhase(ChemicalPhase chemicalPhase)
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
					throw new NotSupportedException("Plasma isn't supported.");
				default:
					throw new ArgumentException(nameof(chemicalPhase));
			}
		}
	}
}
