using ImGuiNET;

namespace Space_Refinery_Game_Renderer
{
	public partial class BatchRenderable
	{
		// TODO: what is the purpose of this static list? should it be kept in main game or similar? or by graphics system? or manager for batch renderables? is it just here for debug? if so, maybe change the debug menu to somehow work with a provided gamedata?
		public static readonly List<BatchRenderable> BatchRenderables = new();

		private static object syncRoot = new();

		private static void RegisterBatchRenderable(BatchRenderable batchRenderable)
		{
			lock (syncRoot)
			{
				BatchRenderables.Add(batchRenderable);
			}
		}

		private static void UnregisterBatchRenderable(BatchRenderable batchRenderable)
		{
			lock (syncRoot)
			{
				BatchRenderables.Remove(batchRenderable);
			}
		}

		public static void DoDebugUI()
		{
			//ImGui.SetNextWindowSizeConstraints(new(100, 100), new(1000, 1000));
			ImGui.Begin("Renderable Debugging");
			lock (syncRoot)
			{
				ImGui.Columns(BatchRenderables.Count);
				foreach (var batchRenderable in BatchRenderables)
				{
					DoDebugUIForBatchRenderable(batchRenderable);
					ImGui.NextColumn();
					ImGui.SetColumnWidth(ImGui.GetColumnIndex(), 500);
				}
			}
			ImGui.End();
		}

		private static void DoDebugUIForBatchRenderable(BatchRenderable batchRenderable)
		{
			lock (batchRenderable.SyncRoot)
			{
				ImGui.BulletText(batchRenderable.Name);
				bool shouldDraw = batchRenderable.ShouldDraw;
				ImGui.Checkbox("Should draw", ref shouldDraw);
				ImGui.Text("Internal transforms count: " + batchRenderable.TransformsCount);
				ImGui.Text("Internal capacity: " + batchRenderable.currentCapacity);
				ImGui.Text("Transforms: " + batchRenderable.transformsDictionary.Count);
				ImGui.Text("Available indexes: " + batchRenderable.availableIndexesQueue.Count);
			}
		}
	}
}
