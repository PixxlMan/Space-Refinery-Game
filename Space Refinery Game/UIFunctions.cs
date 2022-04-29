using ImGuiNET;

namespace Space_Refinery_Game
{
	public static class UIFunctions
	{
		public static void DoSelector<T>(ICollection<T> selectables, ref int selection, out bool hasSelection, out T selected) where T : IUIInspectable
		{
			hasSelection = false;

			selected = default;

			ImGui.Indent();
			{
				int selectionIndex = 0;
				foreach (T selectable in selectables)
				{
					selectable.DoUIInspectorReadonly();

					if (ImGui.Selectable($"Select {selectionIndex}", selectionIndex == selection) || selectionIndex == selection)
					{
						selection = selectionIndex;

						hasSelection = true;

						selected = selectable;
					}

					selectionIndex++;
				}			
			}
			ImGui.Unindent();

			if (!hasSelection)
			{
				selection = -1;
			}
		}
	}
}
