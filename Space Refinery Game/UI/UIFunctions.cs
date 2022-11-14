using ImGuiNET;
using System.Numerics;

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

		//public static void DoSelector<TEnum>(ref int selection, string label, Guid guid, out bool hasSelection, out TEnum selected)
		//	where TEnum : struct, Enum
		//{
		//	hasSelection = false;

		//	selected = default;

		//	if (BeginButtonDropDown(label))
		//	{
		//		int selectionIndex = 0;
		//		foreach (TEnum selectable in Enum.GetValues<TEnum>())
		//		{
		//			if (ImGui.Selectable($"{selectable}", selectionIndex == selection) || selectionIndex == selection)
		//			{
		//				selection = selectionIndex;

		//				hasSelection = true;

		//				selected = selectable;
		//			}

		//			selectionIndex++;
		//		}

		//		EndButtonDropDown();
		//	}

		//	if (!hasSelection)
		//	{
		//		selection = -1;
		//	}
		//}

		/// <summary>
		/// The purpose of using this instead of DoSelector<TEnum> is to allow choosing which enums to allow with selectables collection.
		/// </summary>
		public static void DoSelectorEnums<TEnum>(IReadOnlyList<TEnum> selectables, string label, Guid guid, ref int selection, out bool hasSelection, out TEnum selected)
			where TEnum : struct, Enum
		{
			hasSelection = false;

			selected = default;

			if (BeginButtonDropDown(label))
			{
				int selectionIndex = 0;
				foreach (TEnum selectable in selectables)
				{
					if (ImGui.Selectable($"{selectable}", selectionIndex == selection) || selectionIndex == selection)
					{
						selection = selectionIndex;

						hasSelection = true;

						selected = selectable;
					}

					selectionIndex++;
				}

				EndButtonDropDown();
			}
			else
			{
				hasSelection = selection != -1;
				selected = selectables[selection];
				return;
			}

			if (!hasSelection)
			{
				selection = -1;
				selected = default;
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

		public static bool BeginButtonDropDown(string label) => BeginButtonDropDown(label, new(20, 20));

		public static bool BeginButtonDropDown(string label, Vector2 buttonSize) // https://github.com/ocornut/imgui/issues/474
		{
			ImGui.SameLine(0, 0);

			//ImGuiWindow* window = GetCurrentWindow();
			//ImGuiState & g = *GImGui;
			//ImGuiStylePtr style = ImGui.GetStyle();

			Vector2 windowPos = ImGui.GetWindowPos();

			float x = ImGui.GetCursorPosX();
			float y = ImGui.GetCursorPosY();

			Vector2 size = new(20, buttonSize.Y);
			bool pressed = ImGui.Button("##", size);

			// Arrow
			Vector2 center = new(windowPos.X + x + 10, windowPos.Y + y + buttonSize.Y / 2);
			float r = 8;
			center.Y -= r * 0.25f;
			Vector2 a = center + new Vector2(0, 1) * r;
			Vector2 b = center + new Vector2(-0.866f, -0.5f) * r;
			Vector2 c = center + new Vector2(0.866f, -0.5f) * r;

			ImGui.GetWindowDrawList().AddTriangleFilled(a, b, c, ImGui.GetColorU32(ImGuiCol.Text));

			// Popup

			Vector2 popupPos;

			popupPos.X = windowPos.X + x - buttonSize.X;
			popupPos.Y = windowPos.Y + y + buttonSize.Y;

			ImGui.SetNextWindowPos(popupPos);

			if (pressed)
			{
				ImGui.OpenPopup(label);
			}

			if (ImGui.BeginPopup(label))
			{
				//ImGui.PushStyleColor(ImGuiCol.FrameBg, style.Colors[ImGuiCol.Button]);
				//ImGui.PushStyleColor(ImGuiCol.WindowBg, style.Colors[ImGuiCol.Button]);
				//ImGui.PushStyleColor(ImGuiCol.ChildBg, style.Colors[ImGuiCol.Button]);
				return true;
			}

			return false;
		}

		public static void EndButtonDropDown()
		{
			//ImGui.PopStyleColor(3);
			ImGui.EndPopup();
		}
	}
}
