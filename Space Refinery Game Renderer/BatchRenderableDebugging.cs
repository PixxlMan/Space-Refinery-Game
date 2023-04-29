using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace Space_Refinery_Game_Renderer
{
	public partial class BatchRenderable
	{
		private static List<BatchRenderable> batchRenderables = new();

		private static object syncRoot = new();

		private static void RegisterBatchRenderable(BatchRenderable batchRenderable)
		{
			lock (syncRoot)
			{
				batchRenderables.Add(batchRenderable);
			}
		}

		private static void UnregisterBatchRenderable(BatchRenderable batchRenderable)
		{
			lock (syncRoot)
			{
				batchRenderables.Remove(batchRenderable);
			}
		}

		public static void DoDebugUI()
		{
			//ImGui.SetNextWindowSizeConstraints(new(100, 100), new(1000, 1000));
			ImGui.Begin("Renderable Debugging");
			lock (syncRoot)
			{
				ImGui.Columns(batchRenderables.Count);
				foreach (var batchRenderable in batchRenderables)
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
				ImGui.Text("Internal transforms count: " + batchRenderable.transforms.Count);
				ImGui.Text("Internal capacity: " + batchRenderable.currentCapacity);
				ImGui.Text("Transforms: " + batchRenderable.transformsDictionary.Count);
				ImGui.Text("Available indexes: " + batchRenderable.availableIndexesQueue.Count);
			}
		}
	}
}
