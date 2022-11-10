using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public sealed class DebugSettings
	{
		public Dictionary<string, IDebugSetting> DebugSettingsDictionary = new();

		public TSetting AccessSetting<TSetting>(string name)
			where TSetting : struct, IDebugSetting
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

		public TSetting AccessSetting<TSetting>(string name, TSetting defaultSettingValue)
			where TSetting : struct, IDebugSetting
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
