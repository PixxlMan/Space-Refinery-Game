namespace Space_Refinery_Game
{
	public interface IDebugSetting
	{
		public string SettingText { get; set; }

		public abstract void DrawUIElement();
	}
}