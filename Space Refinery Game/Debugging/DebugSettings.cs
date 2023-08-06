namespace Space_Refinery_Game
{
	public sealed class DebugSettings
	{
		public Dictionary<string, IDebugSetting> DebugSettingsDictionary = new();

		private object syncRoot = new();

		public TSetting AccessSetting<TSetting>(string name)
			where TSetting : IDebugSetting
		{
			lock (syncRoot)
			{
				if (DebugSettingsDictionary.ContainsKey(name))
				{
					return (TSetting)DebugSettingsDictionary[name];
				}
				else
				{
					var setting = default(TSetting);

					setting.SettingText = name;

					DebugSettingsDictionary.Add(name, setting);

					return setting;
				}
			}
		}

		public TSetting AccessSetting<TSetting>(string name, TSetting defaultSettingValue)
			where TSetting : IDebugSetting
		{
			lock (syncRoot)
			{
				if (DebugSettingsDictionary.ContainsKey(name))
				{
					return (TSetting)DebugSettingsDictionary[name];
				}
				else
				{
					defaultSettingValue.SettingText = name;

					DebugSettingsDictionary.Add(name, defaultSettingValue);

					return defaultSettingValue;
				}
			}
		}
	}
}
