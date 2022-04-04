using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class DebugSettings
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
	}
}
