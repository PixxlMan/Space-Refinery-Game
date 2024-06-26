﻿using ImGuiNET;

namespace Space_Refinery_Engine;

public class LevelObjectInformationProvider(LevelObject levelObject) : IInformationProvider
{
	public LevelObject LevelObject { get; private set; } = levelObject;

	public string Name => LevelObject.LevelObjectType.Name;

	public virtual void InformationUI()
	{
		ImGui.Spacing();

		if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Show debug information in information provider"))
		{
			ImGui.Text("GUID: " + LevelObject.SerializableReference.ToString());

			ImGui.Text("Postition: " + LevelObject.Transform.Position.ToString());

			ImGui.Text("Orientation: " + LevelObject.Transform.Rotation.ToString());

			ImGui.Text("LevelObject type: " + LevelObject.LevelObjectType.Name);

			if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Show rendering debug information in information provider"))
			{
				ImGui.Text("Batch renderer: " + LevelObject.LevelObjectType.BatchRenderable.Name);

				ImGui.Text("Rendering index: " + LevelObject.LevelObjectType.BatchRenderable.DebugGetRenderableIndex(LevelObject));
			}
		}
	}
}