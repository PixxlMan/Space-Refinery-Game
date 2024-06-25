using FixedPrecision;
using Veldrid;

namespace Space_Refinery_Game.Renderer;

internal readonly struct DebugRenderable(Mesh mesh, DeviceBuffer transformationBuffer, DeviceBuffer colorBuffer) : IRenderable, IDisposable
{
	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.SetVertexBuffer(0, mesh.VertexBuffer);
		commandList.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
		commandList.SetVertexBuffer(1, colorBuffer);
		commandList.SetVertexBuffer(2, transformationBuffer);

		commandList.DrawIndexed(mesh.IndexCount);
	}

	public void Dispose()
	{
		transformationBuffer.Dispose();
		colorBuffer.Dispose();
	}
}
