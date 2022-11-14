using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace Space_Refinery_Game
{
	public sealed class ComboDebugSetting<T> : IDebugSetting
		where T : IUIInspectable
	{
		private object syncRoot = new();

		private T? selected;
		public T? Selected { get { lock (syncRoot) return selected; } }

		private bool hasSelection;
		public bool HasSelection { get { lock (syncRoot) return hasSelection; } }

		public ICollection<T> SelectionCollection { get; }

		private int selectionIndex = 0;

		private Guid guid = Guid.NewGuid();

		public ComboDebugSetting(ICollection<T> selectionCollection)
		{
			SelectionCollection = selectionCollection;
		}

		public ComboDebugSetting(IList<T> selectionCollection, T initiallySelected)
		{
			SelectionCollection = selectionCollection;
			selectionIndex = selectionCollection.IndexOf(initiallySelected);
			selected = initiallySelected;
		}

		public string SettingText { get; set; }

		public void DrawUIElement()
		{
			lock (syncRoot)
			{
				ImGui.TextUnformatted(SettingText);

				UIFunctions.DoSelector(SelectionCollection, guid, ref selectionIndex, out hasSelection, out selected);
			}
		}

		public static implicit operator T(ComboDebugSetting<T> setting) => setting.Selected;
	}

	public sealed class EnumDebugSetting<T> : IDebugSetting
		where T : struct, Enum
	{
		public T? Selected { get; private set; }

		public bool HasSelection { get; private set; }

		T[] enumValues;

		private int selectionIndex = 0;

		private Guid guid = Guid.NewGuid();

		public EnumDebugSetting(T initiallySelected)
		{
			enumValues = Enum.GetValues<T>();

			selectionIndex = Array.IndexOf(enumValues, initiallySelected);

			Selected = initiallySelected;
			HasSelection = true;
			SettingText = null;
		}

		public string SettingText { get; set; }

		public void DrawUIElement()
		{
			if (enumValues is null)
			{
				enumValues = Enum.GetValues<T>();
			}

			ImGui.TextUnformatted(SettingText);

			UIFunctions.DoSelectorEnums<T>(enumValues, guid, ref selectionIndex, out var hasSelection, out var selected);
			HasSelection = hasSelection;
			Selected = selected;
		}

		public static implicit operator T(EnumDebugSetting<T> setting) => setting.Selected.Value;

		public static implicit operator EnumDebugSetting<T>(T value) => new(value);
	}
}
