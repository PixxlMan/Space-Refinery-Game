using ImGuiNET;
using System.Xml;

namespace Space_Refinery_Game
{
	public struct SwitchSettingValue : ISettingValue
	{
		public bool SwitchValue;

		public SwitchSettingValue(bool switchValue)
		{
			SwitchValue = switchValue;
		}

		public static implicit operator bool(SwitchSettingValue switchSettingValue)
		{
			return switchSettingValue.SwitchValue;
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.Serialize(SwitchValue, nameof(SwitchValue));
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SwitchValue = reader.DeserializeBoolean(nameof(SwitchValue));
		}

		public void ShowValueUI(Setting setting)
		{
			UIFunctions.PushDisabled();

			bool switchValue = SwitchValue;
			ImGui.Checkbox(string.Empty, ref switchValue);

			UIFunctions.PopEnabledOrDisabledState();
		}
	}

	public sealed class SwitchSetting : Setting
	{
		private SwitchSetting() : base()
		{

		}

		public SwitchSetting(SwitchSettingValue defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public override void Accept()
		{
			if (Dirty)
			{
				value.SwitchValue = (bool)uiValue;

				ValueChanged();
			}
		}

		public override void Cancel()
		{
			uiValue = Value.SwitchValue;
		}

		public override void DoUI()
		{
			ImGui.Checkbox(Name, ref uiValue);

			if (uiValue != lastValue)
			{
				SettingChanged?.Invoke(new SwitchSettingValue(uiValue));
			}

			lastValue = uiValue;
		}

		public override void ValueChanged()
		{
			uiValue = Value.SwitchValue;

			AcceptedSettingChange?.Invoke(Value);
		}

		bool uiValue;

		bool lastValue;

		public override bool Dirty => (bool)uiValue != Value;

		private SwitchSettingValue value;

		public SwitchSettingValue Value { get => value; private set { this.value = value; ValueChanged(); } }

		public override ISettingValue SettingValue { get => Value; set => Value = (SwitchSettingValue)value; }

		public override event Action<ISettingValue> AcceptedSettingChange;

		public override event Action<ISettingValue> SettingChanged;

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);
		}

		public override string GetLimitsDescription() => string.Empty;
	}
}
