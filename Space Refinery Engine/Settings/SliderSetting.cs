﻿using ImGuiNET;
using System.Xml;

namespace Space_Refinery_Engine
{
	public struct SliderSettingValue : ISettingValue
	{
		public DecimalNumber SliderValue;

		public SliderSettingValue(DecimalNumber sliderValue)
		{
			SliderValue = sliderValue;
		}

		public static implicit operator DecimalNumber(SliderSettingValue sliderSettingValue)
		{
			return sliderSettingValue.SliderValue;
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.Serialize(SliderValue, nameof(SliderValue));
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SliderValue = reader.DeserializeDecimalNumber(nameof(SliderValue));
		}

		public void ShowValueUI(Setting setting)
		{
			var sliderSetting = (SliderSetting)setting;

			UIFunctions.PushDisabled();

			float sliderValue = SliderValue.ToFloat();
			ImGui.SliderFloat(string.Empty, ref sliderValue, sliderSetting.Min.ToFloat(), sliderSetting.Max.ToFloat());

			UIFunctions.PopEnabledOrDisabledState();
		}
	}

	public sealed class SliderSetting : Setting
	{
		private SliderSetting() : base()
		{

		}

		public SliderSetting(DecimalNumber min, DecimalNumber max, SliderSettingValue defaultValue, string sliderValueSuffix)
		{
			Min = min;
			Max = max;
			DefaultValue = defaultValue;
			SliderValueSuffix = sliderValueSuffix;
		}

		public override void Accept()
		{
			if (Dirty)
			{
				value.SliderValue = (DecimalNumber)uiValue;

				ValueChanged();
			}
		}

		public override void Cancel()
		{
			uiValue = Value.SliderValue.ToFloat();
		}

		public override void DoUI()
		{
			ImGui.SliderFloat(Name, ref uiValue, Min.ToFloat(), Max.ToFloat());

			if (uiValue != lastValue)
			{
				SettingChanged?.Invoke(new SliderSettingValue(uiValue));
			}

			lastValue = uiValue;
		}

		public override void ValueChanged()
		{
			uiValue = Value.SliderValue.ToFloat();

			AcceptedSettingChange?.Invoke(Value);
		}

		float uiValue;

		float lastValue;

		public DecimalNumber Min;

		public DecimalNumber Max;

		public string SliderValueSuffix;

		public override bool Dirty => (DecimalNumber)uiValue != Value;

		private SliderSettingValue value;

		public SliderSettingValue Value { get => value; private set { this.value = value; ValueChanged(); } }

		public override ISettingValue SettingValue { get => Value; set => Value = (SliderSettingValue)value; }

		public override event Action<ISettingValue> AcceptedSettingChange;

		public override event Action<ISettingValue> SettingChanged;

		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.Serialize(Min, nameof(Min));
			writer.Serialize(Max, nameof(Max));
			writer.Serialize(SliderValueSuffix, nameof(SliderValueSuffix));
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			Min = reader.DeserializeDecimalNumber(nameof(Min));
			Max = reader.DeserializeDecimalNumber(nameof(Max));
			SliderValueSuffix = reader.ReadString(nameof(SliderValueSuffix));
		}

		public override string GetLimitsDescription() => $"Max: {Max}{SliderValueSuffix}, Min: {Min}{SliderValueSuffix}";
	}
}
