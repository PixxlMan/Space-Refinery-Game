using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	[DataContract]
	public class Settings
	{
		[DataMember]
		private Dictionary<string, ISetting> settings = new();

		[DataMember]
		private Dictionary<string, ISettingOptions> settingsOptions = new();

		public void SetSettingOptions(string name, ISettingOptions options)
		{
			if (!settingsOptions.ContainsKey(name))
			{
				settingsOptions.Add(name, options);
			}

			if (settings.ContainsKey(name))
			{
				settings[name].Options = options;
			}
		}

		public void RegisterToSetting<TSetting>(string name, Action<TSetting> settingChangeAcceptedHandler, Action<TSetting>? settingChangedHandler = null, ISetting? defaultValue = null)
			where TSetting : ISetting
		{
			ISetting setting;

			if (!settings.ContainsKey(name))
			{
				if (defaultValue is not null)
				{
					setting = defaultValue;
					setting.SetUp();
				}
				else
				{
					setting = TSetting.Create();
					setting.SetUp();
				}

				if (settingsOptions.ContainsKey(name))
				{
					setting.Options = settingsOptions[name];
				}

				settings.Add(name, setting);
			}
			else
			{
				setting = settings[name];
			}

			setting.AcceptedSettingChange += (ISetting setting) => settingChangeAcceptedHandler((TSetting)setting);

			if (settingChangedHandler is not null)
			{
				setting.SettingChanged += (ISetting setting) => settingChangedHandler((TSetting)setting);
			}

			settingChangeAcceptedHandler((TSetting)setting);
		}

		public void DoSettingsUI()
		{
			bool dirty = false;

			foreach (var nameSettingPair in settings)
			{
				ImGui.Text(nameSettingPair.Key);

				ImGui.SameLine();

				ImGui.PushID(nameSettingPair.Value.Guid.ToString());
					nameSettingPair.Value.DoUI();
				ImGui.PopID();

				if (nameSettingPair.Value.Dirty)
				{
					dirty = true;
				}
			}

			if (!dirty)
			{
				UIFunctions.PushDisabled();
			}
			if (ImGui.Button("Accept"))
			{
				foreach (var setting in settings.Values)
				{
					setting.Accept();
				}
			}

			ImGui.SameLine();

			if (ImGui.Button("Cancel"))
			{
				foreach (var setting in settings.Values)
				{
					setting.Cancel();
				}
			}
			if (!dirty)
			{
				UIFunctions.PopDisabled();
			}
		}

		public void Serialize(string path)
		{
			using FileStream stream = File.OpenWrite(path);

			DataContractSerializer dataContractSerializer = new(typeof(Settings));

			dataContractSerializer.WriteObject(stream, this);
		}
	}
}
