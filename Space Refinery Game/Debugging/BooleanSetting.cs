using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace Space_Refinery_Game
{
	public struct BooleanSetting : IDebugSetting
	{
		public bool Value;

		public string SettingText { get; set; }

		public void DrawUIElement()
		{
			ImGui.Checkbox(SettingText, ref Value);
		}

		public static implicit operator bool(BooleanSetting booleanSetting) => booleanSetting.Value;
	}
}
