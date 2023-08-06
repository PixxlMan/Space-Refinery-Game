using ImGuiNET;
using Space_Refinery_Utilities;

namespace Space_Refinery_Game
{
	public class SaveCreatorUIPrototype
	{
		private string refineryName = randomNames.SelectRandom();

		private int currentPreset = 0;

		private static (string presetName, string presetDescription)[] presets =
			new[]
				{
					("Standard", "The standard experience, recomended for beginners and more experienced players alike"),
					("Simple", "An easier experience with reduced economic challenges and reduced engineering challenges and simpler chemical reactions"),
					("Tycoon", "A more challenging economy"),
					("Engineer", "A more challenging engineering aspect"),
					("Realistic", "Tuned for additional realism"),
					("Hard", "A more challenging experience"), };

		private static string[] randomNames = new[] { "Best refinery", "Nice refinery", "RefineryMcRefineFace", "Chiron beta prime", };

		public void DoUI()
		{
			if (ImGui.Begin("Prototype - Create new save"))
			{
				ImGui.InputText("Refinery name", ref refineryName, 64);
				ImGui.SameLine();
				if (ImGui.Button("Randomize"))
				{
					refineryName = randomNames.SelectRandomNew(refineryName);
				}

				if (string.Equals(refineryName, "Chiron beta prime", StringComparison.CurrentCultureIgnoreCase))
				{
					ImGui.Text("Insert easter egg here.");
				}

				DoPresetSelector();

				if (ImGui.CollapsingHeader("Settings"))
				{
					UIFunctions.BeginSub();
					/*foreach (var saveSetting in saveSettings)
					{
						saveSetting.DoUI();
					}*/
					UIFunctions.EndSub();
				}

				ImGui.End();
			}
		}

		private void DoPresetSelector()
		{
			for (int i = 0; i < presets.Length; i++)
			{
				string preset = presets[i].presetName;
				ImGui.BeginGroup();
				{
					if (ImGui.RadioButton(preset, currentPreset == i/* && UIFunctions.IsEnabled*/))
					{					
						currentPreset = i;
					}
					ImGui.EndGroup();
				}
				if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled | ImGuiHoveredFlags.AnyWindow | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem | ImGuiHoveredFlags.AllowWhenOverlapped))
					ImGui.SetTooltip(presets[i].presetDescription);

				ImGui.SameLine();
			}
			ImGui.Button("More presets");
		}
	}
}
