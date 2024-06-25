using ImGuiNET;

namespace Space_Refinery_Game.Renderer;

public partial class BatchRenderable
{
	// TODO: what is the purpose of this static list? should it be kept in main game or similar? or by graphics system? or manager for batch renderables? is it just here for debug? if so, maybe change the debug menu to somehow work with a provided gamedata?
	public static List<BatchRenderable> BatchRenderables { get; private set; } = new();

	private static object staticSyncRoot = new();

	private static void RegisterBatchRenderable(BatchRenderable batchRenderable)
	{
		lock (staticSyncRoot)
		{
			BatchRenderables.Add(batchRenderable);
		}
	}

	private static void UnregisterBatchRenderable(BatchRenderable batchRenderable)
	{
		lock (staticSyncRoot)
		{
			BatchRenderables.Remove(batchRenderable);
		}
	}

	/// <summary>
	/// Returns the internal batch renderer index associated with an object, or -1 if the object is not associated with any index.
	/// </summary>
	/// <remarks>Used for debugging purposes only, do not rely on this method's behaviour or results.</remarks>
	/// <returns>The internal renderer index associated with the object, or -1 if there is no index associated with the object.</returns>
	[Obsolete("This method is intended for debugging purposes only!")]
	public int DebugGetRenderableIndex(object associatedObject)
	{
		lock (syncRoot)
		{
			if (!transformsDictionary.TryGetValue(associatedObject, out var index))
			{
				index = -1;
			}

			return index;
		}
	}

	public static void DoDebugUI()
	{
		ImGui.Begin("Renderable Debugging");
		lock (staticSyncRoot)
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
		lock (batchRenderable.syncRoot)
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
