using ImGuiNET;

namespace Space_Refinery_Game
{
	public struct SliderDebugSetting : IDebugSetting
	{
		public SliderDebugSetting(DecimalNumber value) : this(value, 0, 1)
		{

		}

		public SliderDebugSetting(DecimalNumber value, DecimalNumber min, DecimalNumber max) // Simple intializer to make creating a SliderDebugSetting of a certain value more convenient.
		{
			Value = value;
			Min = min;
			Max = max;
			SettingText = null;
		}

		private float value;
		public DecimalNumber Value { get => value; set => this.value = value.ToFloat(); }

		private float min;
		public DecimalNumber Min { get => min; set => this.min = value.ToFloat(); }

		private float max;
		public DecimalNumber Max { get => max; set => this.max = value.ToFloat(); }

		public string SettingText { get; set; }

		public void DrawUIElement()
		{
			ImGui.SliderFloat(SettingText, ref value, min, max);
		}

		public static implicit operator DecimalNumber(SliderDebugSetting sliderSetting) => sliderSetting.Value;

		public static implicit operator SliderDebugSetting(DecimalNumber value) => new(value);
	}
}
