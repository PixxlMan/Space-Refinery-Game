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
	internal readonly struct DebugRenderable : IRenderable
	{
		public static DebugRenderable Create(Mesh mesh, Transform transform, RgbaFloat color, GraphicsDevice gd, ResourceFactory factory)
		{
			DeviceBuffer transformationBuffer = factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));

			gd.UpdateBuffer(transformationBuffer, 0, ((ITransformable)transform).GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

			DeviceBuffer colorBuffer = factory.CreateBuffer(new BufferDescription((uint)RgbaFloat.SizeInBytes, BufferUsage.VertexBuffer));

			gd.UpdateBuffer(colorBuffer, 0, color);

			return new(mesh, transformationBuffer, colorBuffer);
		}

		private DebugRenderable(Mesh mesh, DeviceBuffer transformationBuffer, DeviceBuffer colorBuffer)
		{
			Mesh = mesh;
			TransformationBuffer = transformationBuffer;
			ColorBuffer = colorBuffer;
		}

		public readonly Mesh Mesh;

		public readonly DeviceBuffer TransformationBuffer;

		public readonly DeviceBuffer ColorBuffer;

		public void AddDrawCommands(CommandList commandList)
		{
			commandList.SetVertexBuffer(0, Mesh.VertexBuffer);
			commandList.SetIndexBuffer(Mesh.IndexBuffer, Mesh.IndexFormat);
			commandList.SetVertexBuffer(1, ColorBuffer);
			commandList.SetVertexBuffer(2, TransformationBuffer);

			commandList.DrawIndexed(Mesh.IndexCount);
		}
	}
}
