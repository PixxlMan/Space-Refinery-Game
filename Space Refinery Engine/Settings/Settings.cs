using ImGuiNET;
using System.Collections.Concurrent;
using System.Xml;

namespace Space_Refinery_Engine // Is this really thread safe? It's accessed statically, so it ought to be.
{
	public sealed class Settings : IEntitySerializable
	{
		private SerializationReferenceHandler settingsReferenceHandler = new();
		private ConcurrentDictionary<string, Setting> settings = new();

		private GameData gameData;

		public Settings(GameData gameData)
		{
			settingsReferenceHandler.EnterAllowEventualReferenceMode(false);

			this.gameData = gameData;
		}

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

			SaveSettingsToSettingsFile();
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

		public void SaveSettingsToSettingsFile()
		{
			Logging.Log("Saving setting values");

			Directory.CreateDirectory(settingValuesDirectoryPath);

			using var stream = File.Create(settingValuesPath);

			using var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });

			SerializeSettingValues(writer, new(gameData));

			writer.Flush();
			writer.Close();
			stream.Flush(true);
			stream.Close();
			writer.Dispose();
			stream.Dispose();
		}

		public void LoadSettingValuesFromSettingsFile()
		{
			Logging.Log("Loading setting values");

			if (File.Exists(settingValuesPath))
			{
				using var reader = XmlReader.Create(settingValuesPath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });

				DeserializeSettingValues(reader, new(gameData, MainGame.EngineExtension.AssetsPath));
			}
			else
			{
				SetDefault();
			}

			EndDeserialization();

			AcceptAllSettings();
		}

		private void SerializeSettingValues(XmlWriter writer, SerializationData serializationData)
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

		private void DeserializeSettingValues(XmlReader reader, SerializationData serializationData)
		{
			reader.DeserializeCollection((r) =>
			{
				r.ReadStartElement(nameof(ISettingValue));
				{
					var setting = r.DeserializeKnownReference<Setting>(settingsReferenceHandler, "SettingReference");

					setting.SettingValue = (ISettingValue)r.DeserializeEntitySerializableWithEmbeddedType(serializationData, settingsReferenceHandler);

					settings[setting.Name] = setting;
				}
				r.ReadEndElement();
			}, "SettingValues");
		}

		public void SerializeState(XmlWriter writer, SerializationData serializationData)
		{
			SerializeSettingValues(writer, serializationData);
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			DeserializeSettingValues(reader, serializationData);
		}
	}
}
