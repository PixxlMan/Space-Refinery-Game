using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public struct SliderSetting : ISetting
	{
		public event Action<ISetting> AcceptedSettingChange;
		public event Action<ISetting> SettingChanged;

		public static ISetting Create()
		{
			return new SliderSetting();
		}

		public void Accept()
		{
			if (Dirty)
			{
				Value = (FixedDecimalInt4)uiValue;

				AcceptedSettingChange?.Invoke(this);
			}
		}

		public void Cancel()
		{
			uiValue = Value.ToFloat();
		}

		public void DoUI()
		{
			ImGui.SliderFloat(string.Empty, ref uiValue, ((SliderSettingOptions)Options).Min.ToFloat(), ((SliderSettingOptions)Options).Max.ToFloat());

			if (uiValue != lastValue)
			{
				SettingChanged?.Invoke(this);
			}

			lastValue = uiValue;
		}

		public FixedDecimalInt4 Value;

		float uiValue;

		float lastValue;

		public bool Dirty => (FixedDecimalInt4)uiValue != Value;

		public ISettingOptions Options { get; set; } = new SliderSettingOptions(0, 1000);
	}
}
