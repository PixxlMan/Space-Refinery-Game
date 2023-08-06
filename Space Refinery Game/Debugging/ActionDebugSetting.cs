using ImGuiNET;

namespace Space_Refinery_Game
{
	public sealed class ActionDebugSetting : IDebugSetting
	{
		public ActionDebugSetting(Action action)
		{
			this.action = action;
			SettingText = null;
		}

		public Action action;

		public string SettingText { get; set; }

		public void DrawUIElement()
		{
			if (ImGui.Button(SettingText))
			{
				action?.Invoke();
			}
		}

		public static implicit operator ActionDebugSetting(Action action) => new(action);
	}
}
