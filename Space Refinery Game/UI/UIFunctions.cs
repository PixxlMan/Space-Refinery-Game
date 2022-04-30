using ImGuiNET;

namespace Space_Refinery_Game
{
	public static class UIFunctions
	{
		public static void BeginSub()
		{
			ImGui.PushID(0);
			ImGui.BeginGroup();
			ImGui.Indent();
		}

		public static void EndSub()
		{
			ImGui.Unindent();
			ImGui.EndGroup();
			ImGui.PopID();
		}

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

		public static void DoListManipulation<T>(IList<T> collection, bool allowEditingItems) where T : IUIInspectable
		{
			List<int> indexesToRemove = new();

			ImGui.Indent();
			{
				int currentIndex = 0;
				foreach (var item in collection)
				{
					if (allowEditingItems)
					{
						item.DoUIInspectorEditable();
					}
					else
					{
						item.DoUIInspectorReadonly();
					}

					if (ImGui.Button("Delete"))
					{
						indexesToRemove.Add(currentIndex);
					}

					currentIndex++;
				}

				foreach (var index in indexesToRemove)
				{
					collection.RemoveAt(index);
				}
			}
			ImGui.Unindent();
		}
	}
}
