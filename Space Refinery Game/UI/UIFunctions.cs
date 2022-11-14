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

		public static void BeginSub(Guid guid)
		{
			ImGui.PushID(guid.ToString());
			ImGui.BeginGroup();
			ImGui.Indent();
		}

		public static void EndSub()
		{
			ImGui.Unindent();
			ImGui.EndGroup();
			ImGui.PopID();
		}

		public static void PushDisabled()
		{
			//ImGui.PushStyleColor(ImGuiCol.Button, RgbaByte.LightGrey);
			ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
		}

		public static void PopDisabled()
		{
			ImGui.PopStyleVar();
		}

		public static void DoSelector<T>(ICollection<T> selectables, Guid guid, ref int selection, out bool hasSelection, out T selected)
			where T : IUIInspectable
		{
			hasSelection = false;

			selected = default;

			BeginSub(guid);
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
			EndSub();

			if (!hasSelection)
			{
				selection = -1;
			}
		}

		public static void DoSelector<TEnum>(ref int selection, Guid guid, out bool hasSelection, out TEnum selected)
			where TEnum : struct, Enum
		{
			hasSelection = false;

			selected = default;

			BeginSub(guid);
			{
				int selectionIndex = 0;
				foreach (TEnum selectable in Enum.GetValues<TEnum>())
				{
					selectable.ToString();

					if (ImGui.Selectable($"Select {selectionIndex}", selectionIndex == selection) || selectionIndex == selection)
					{
						selection = selectionIndex;

						hasSelection = true;

						selected = selectable;
					}

					selectionIndex++;
				}			
			}
			EndSub();

			if (!hasSelection)
			{
				selection = -1;
			}
		}

		/// <summary>
		/// The purpose of using this instead of DoSelector<TEnum> is to allow choosing which enums to allow with selectables collection.
		/// </summary>
		public static void DoSelectorEnums<TEnum>(ICollection<TEnum> selectables, Guid guid, ref int selection, out bool hasSelection, out TEnum selected)
			where TEnum : struct, Enum
		{
			hasSelection = false;

			selected = default;

			BeginSub(guid);
			{
				int selectionIndex = 0;
				foreach (TEnum selectable in selectables)
				{
					ImGui.TextUnformatted(selectable.ToString());

					if (ImGui.Selectable($"Select {selectionIndex}", selectionIndex == selection) || selectionIndex == selection)
					{
						selection = selectionIndex;

						hasSelection = true;

						selected = selectable;
					}

					selectionIndex++;
				}
			}
			EndSub();

			if (!hasSelection)
			{
				selection = -1;
			}
		}

		public static void DoListManipulation<T>(IList<T> collection, Guid guid, bool allowEditingItems) where T : IUIInspectable
		{
			List<int> indexesToRemove = new();

			BeginSub(guid);
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

					if (ImGui.Button($"Delete {currentIndex}"))
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
			EndSub();
		}
	}
}
