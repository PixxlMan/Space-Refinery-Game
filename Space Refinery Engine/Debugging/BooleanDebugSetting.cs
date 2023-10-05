using ImGuiNET;

namespace Space_Refinery_Engine
{
	public struct BooleanDebugSetting : IDebugSetting
	{
		public BooleanDebugSetting(bool value) // Simple intializer to make creating a BooleanDebugSetting of a certain value more convenient.
		{
			Value = value;
			SettingText = null;
		}

		public bool Value;

		public string SettingText { get; set; }

		public void DrawUIElement()
		{
			ImGui.Checkbox(SettingText, ref Value);
		}

		public static implicit operator bool(BooleanDebugSetting booleanSetting) => booleanSetting.Value;

		public static implicit operator BooleanDebugSetting(bool value) => new(value);
	}
}
