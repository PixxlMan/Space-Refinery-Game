using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game_Renderer
{
	internal readonly struct DebugRenderable : IRenderable, IDisposable
	{
		public DebugRenderable(Mesh mesh, DeviceBuffer transformationBuffer, DeviceBuffer colorBuffer)
		{
			Mesh = mesh;
			TransformationBuffer = transformationBuffer;
			ColorBuffer = colorBuffer;
		}

		public readonly Mesh Mesh;

		public readonly DeviceBuffer TransformationBuffer;

		public readonly DeviceBuffer ColorBuffer;

		public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
		{
			commandList.SetVertexBuffer(0, Mesh.VertexBuffer);
			commandList.SetIndexBuffer(Mesh.IndexBuffer, Mesh.IndexFormat);
			commandList.SetVertexBuffer(1, ColorBuffer);
			commandList.SetVertexBuffer(2, TransformationBuffer);

			commandList.DrawIndexed(Mesh.IndexCount);
		}

		public void Dispose()
		{
			TransformationBuffer.Dispose();
			ColorBuffer.Dispose();
		}
	}
}
