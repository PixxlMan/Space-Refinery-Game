using ImGuiNET;

namespace Space_Refinery_Engine
{
	public class LevelObjectInformationProvider : IInformationProvider
	{
		public LevelObject LevelObject;

		public LevelObjectInformationProvider(LevelObject levelObject)
		{
			LevelObject = levelObject;
		}

		public string Name => LevelObject.LevelObjectType.Name;

		public virtual void InformationUI()
		{
			ImGui.Spacing();

			if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show debug information in information provider"))
			{
				ImGui.Text("GUID: " + LevelObject.SerializableReference.ToString());

				ImGui.Text("Postition: " + LevelObject.Transform.Position.ToString());

				ImGui.Text("Orientation: " + LevelObject.Transform.Rotation.ToString());

				ImGui.Text("LevelObject type: " + LevelObject.LevelObjectType.Name);

				if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Show rendering debug information in information provider"))
				{
					ImGui.Text("Batch renderer: " + LevelObject.LevelObjectType.BatchRenderable.Name);

					ImGui.Text("Rendering index: " + LevelObject.LevelObjectType.BatchRenderable.DebugGetRenderableIndex(LevelObject));
				}
			}
		}
	}
}