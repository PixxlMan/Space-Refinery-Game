using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public struct BooleanSetting : ISetting
	{
		public event Action<ISetting> AcceptedSettingChange;
		public event Action<ISetting> SettingChanged;

		public static ISetting Create()
		{
			return new BooleanSetting();
		}

		public void Accept()
		{
			if (Dirty)
			{
				Value = uiValue;

				AcceptedSettingChange?.Invoke(this);
			}
		}

		public void Cancel()
		{
			uiValue = Value;
		}

		public void DoUI()
		{
			ImGui.Checkbox(string.Empty, ref uiValue);

			if (uiValue != lastValue)
			{
				SettingChanged?.Invoke(this);
			}

			lastValue = uiValue;
		}

		public void SetUp()
		{
			uiValue = Value;
		}

		public bool Value;

		bool uiValue;

		bool lastValue;

		public bool Dirty => uiValue != Value;

		public ISettingOptions Options { get; set; }
	}
}
