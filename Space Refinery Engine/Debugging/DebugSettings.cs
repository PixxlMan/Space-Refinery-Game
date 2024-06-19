using ImGuiNET;
using System.Numerics;

namespace Space_Refinery_Engine
{
	public sealed class DebugSettings
	{
		private Dictionary<string, IDebugSetting> debugSettingsDictionary = new();

		private object syncRoot = new();

		public TSetting AccessSetting<TSetting>(string name)
			where TSetting : struct, IDebugSetting
		{
			lock (syncRoot)
			{
				if (debugSettingsDictionary.ContainsKey(name))
				{
					return (TSetting)debugSettingsDictionary[name];
				}
				else
				{
					var setting = default(TSetting);

					setting.SettingText = name;

					debugSettingsDictionary.Add(name, setting);

					return setting;
				}
			}
		}

		public TSetting AccessSetting<TSetting>(string name, TSetting defaultSettingValue)
			where TSetting : IDebugSetting
		{
			lock (syncRoot)
			{
				if (debugSettingsDictionary.ContainsKey(name))
				{
					return (TSetting)debugSettingsDictionary[name];
				}
				else
				{
					defaultSettingValue.SettingText = name;

					debugSettingsDictionary.Add(name, defaultSettingValue);

					return defaultSettingValue;
				}
			}
		}

		public void DoDebugSettingsUI()
		{
			lock (syncRoot)
			{
				foreach (var debugSetting in debugSettingsDictionary.Values)
				{
					debugSetting.DrawUIElement();
					ImGui.Separator();
				}

				ImGui.End();
			}
		}
	}
}
