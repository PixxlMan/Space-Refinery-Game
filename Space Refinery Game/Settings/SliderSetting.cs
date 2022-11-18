using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public struct SliderSetting : ICreatableSetting
	{
		public event Action<ISetting> AcceptedSettingChange;
		public event Action<ISetting> SettingChanged;

		public static ISetting Create()
		{
			return new SliderSetting() { Guid = Guid.NewGuid() };
		}

		public void Accept()
		{
			if (Dirty)
			{
				Value = (DecimalNumber)uiValue;

				AcceptedSettingChange?.Invoke(this);
			}
		}

		public void Cancel()
		{
			uiValue = Value.ToFloat();
		}

		public void DoUI()
		{
			ImGui.SliderFloat($"{((SliderSettingOptions)Options).Label}##{Guid}", ref uiValue, ((SliderSettingOptions)Options).Min.ToFloat(), ((SliderSettingOptions)Options).Max.ToFloat());

			if (uiValue != lastValue)
			{
				SettingChanged?.Invoke(this);
			}

			lastValue = uiValue;
		}

		public void SetUp()
		{
			uiValue = Value.ToFloat();
		}

		public DecimalNumber Value;

		float uiValue;

		float lastValue;

		public SliderSetting()
		{
		}

		public bool Dirty => (DecimalNumber)uiValue != Value;

		public ISettingOptions Options { get; set; } = new SliderSettingOptions(0, 1000, Guid.NewGuid().ToString());

		public Guid Guid { get; init; } = Guid.NewGuid();
	}
}
