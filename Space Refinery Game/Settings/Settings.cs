using ImGuiNET;
using Space_Refinery_Utilities;
using System.Collections.Concurrent;
using System.Xml;

namespace Space_Refinery_Game // Is this really thread safe? It's accessed statically, so it ought to be.
{
	/// <summary>
	/// Handles all settings that are global for the entire game, such as volume, graphics settings or language etc.
	/// Does not store debug settings or per-save settings, such as difficulty preset.
	/// </summary>
	public sealed class Settings
	{
		private SerializationReferenceHandler settingsReferenceHandler = new();
		private ConcurrentDictionary<string, Setting> settings = new();

		public Settings()
		{
			settingsReferenceHandler.EnterAllowEventualReferenceMode(false);
		}

		public SerializableReference SerializableReference { get; private set; }

		/// <summary>
		/// Registers an action for handling an accepted change and optionally an action for handling any change.
		/// Will register once the reference can be resolved if called during deserialization. Uses eventual references.
		/// </summary>
		/// <typeparam name="TSettingValue">The type of the value expected to be provided by the setting.</typeparam>
		/// <param name="settingName">The reference name of the setting.</param>
		/// <param name="settingChangeAcceptedHandler">An action to be invoked when the settings value has been changed and accepted.</param>
		/// <param name="settingChangedHandler">An action to be invoked when the settings value has been changed, regardless of whether it has been accepted.</param>
		public void RegisterToSettingValue<TSettingValue>(string settingName, Action<TSettingValue> settingChangeAcceptedHandler, Action<TSettingValue>? settingChangedHandler = null)
			where TSettingValue : ISettingValue
		{
			if (settingsReferenceHandler.AllowEventualReferences)
			{
				settingsReferenceHandler.GetEventualReference(settingName, (reference) =>
				{
					var setting = (Setting)reference;

					setting.AcceptedSettingChange += (ISettingValue settingValue) => settingChangeAcceptedHandler((TSettingValue)settingValue);

					if (settingChangedHandler is not null)
					{
						setting.SettingChanged += (ISettingValue settingValue) => settingChangedHandler((TSettingValue)settingValue);
					}

					settingChangeAcceptedHandler((TSettingValue)setting.SettingValue);
				});
			}
			else
			{
				if (!settings.ContainsKey(settingName))
				{
					throw new ArgumentException($"No setting named '{settingName}' exists.", nameof(settingName));
				}

				var setting = settings[settingName];

				setting.AcceptedSettingChange += (ISettingValue settingValue) => settingChangeAcceptedHandler((TSettingValue)settingValue);

				if (settingChangedHandler is not null)
				{
					setting.SettingChanged += (ISettingValue settingValue) => settingChangedHandler((TSettingValue)settingValue);
				}

				settingChangeAcceptedHandler((TSettingValue)setting.SettingValue);
			}
		}

		public void DoSettingsUI()
		{
			bool dirty = false;

			foreach (var nameSettingPair in settings)
			{
				ImGui.Text(nameSettingPair.Key);

				ImGui.SameLine();

				ImGui.PushID(nameSettingPair.Value.SerializableReference.ToString());
				{
					nameSettingPair.Value.DoUI();

					if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled | ImGuiHoveredFlags.AnyWindow | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem | ImGuiHoveredFlags.AllowWhenOverlapped))
					{
						ImGui.BeginTooltip();
						{
							ImGui.Text($"{nameSettingPair.Value.Description}{Environment.NewLine}");
							ImGui.Text($"Default:");
							ImGui.SameLine();
							nameSettingPair.Value.DefaultValue.ShowValueUI(nameSettingPair.Value);
							ImGui.Text(nameSettingPair.Value.GetLimitsDescription());
						}
						ImGui.EndTooltip();
					}
				}
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
				AcceptAllSettings();
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
				UIFunctions.PopEnabledOrDisabledState();
			}

			if (ImGui.Button("Reset to default"))
			{
				SetDefault();
			}
		}

		public void AcceptAllSettings()
		{
			foreach (var setting in settings.Values)
			{
				setting.Accept();
			}

			SaveSettingValues();
		}

		public void AddSetting(Setting setting)
		{
			settingsReferenceHandler.RegisterReference(setting);
			settings.AddUnique(setting.Name, setting, "A setting with this name already exists.");
		}

		public void SetDefault()
		{
			foreach (Setting setting in settings.Values)
			{
				setting.SetDefault();
			}
		}

		internal void EndDeserialization()
		{
			settingsReferenceHandler.ExitAllowEventualReferenceMode();
		}

		private static readonly string settingValuesPath = Path.Combine(Environment.CurrentDirectory, "UserData", "Settings.srh.c.xml");
		private static readonly string settingValuesDirectoryPath = Path.Combine(Environment.CurrentDirectory, "UserData");

		public void SaveSettingValues()
		{
			Logging.Log("Saving setting values");

			Directory.CreateDirectory(settingValuesDirectoryPath);

			using var stream = File.Create(settingValuesPath);

			using var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });

			SerializeSettingValues(writer);

			writer.Flush();
			writer.Close();
			stream.Flush(true);
			stream.Close();
			writer.Dispose();
			stream.Dispose();
		}

		public void LoadSettingValues()
		{
			Logging.Log("Loading setting values");

			if (File.Exists(settingValuesPath))
			{
				using var reader = XmlReader.Create(settingValuesPath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });

				DeserializeSettingValues(reader);
			}
			else
			{
				SetDefault();
			}

			EndDeserialization();

			AcceptAllSettings();
		}

		public void SerializeSettingValues(XmlWriter writer)
		{
			writer.Serialize(settings.Values, (w, st) =>
			{ 
				w.WriteStartElement(nameof(ISettingValue));
				{
					w.SerializeReference(st, "SettingReference");
					w.SerializeWithEmbeddedType(st.SettingValue);
				}
				w.WriteEndElement();
			}, "SettingValues");
		}

		public void DeserializeSettingValues(XmlReader reader)
		{
			reader.DeserializeCollection((r) =>
			{
				r.ReadStartElement(nameof(ISettingValue));
				{
					r.DeserializeReference<Setting>(settingsReferenceHandler, (st) => st.SettingValue = (ISettingValue)r.DeserializeEntitySerializableWithEmbeddedType(null, settingsReferenceHandler), "SettingReference");
				}
				r.ReadEndElement();
			}, "SettingValues");
		}
	}
}
