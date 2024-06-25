using FixedPrecision;
using Veldrid;
using static Space_Refinery_Engine.Renderer.RenderingResources;

namespace Space_Refinery_Engine.Renderer;

public sealed class FullscreenQuad : IRenderable
{
	private ResourceSet quadResourceSet;

	public void CreateDeviceObject(GraphicsWorld graphicsWorld)
	{
		BindableResource[] bindableResources = [graphicsWorld.Factory.CreateTextureView(graphicsWorld.PostProcessing.screenTextureColorOut), graphicsWorld.GraphicsDevice.PointSampler];
		ResourceSetDescription resourceSetDescription = new(TextureLayout, bindableResources);
		quadResourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.SetPipeline(FullscreenQuadPipeline);
		commandList.SetGraphicsResourceSet(0, quadResourceSet);
		commandList.SetVertexBuffer(0, FullscreenQuadVertexBuffer);
		commandList.SetIndexBuffer(FullscreenQuadIndexBuffer, IndexFormat.UInt16);

		commandList.DrawIndexed((uint)FullscreenQuadIndicies.Length);
	}
}
