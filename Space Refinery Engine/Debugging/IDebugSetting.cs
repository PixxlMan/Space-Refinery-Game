namespace Space_Refinery_Engine
{
	public interface IDebugSetting
	{
		public string SettingText { get; set; }

		public abstract void DrawUIElement();
	}
}