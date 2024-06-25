using FixedPrecision;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Engine.Renderer;

public class StarfieldRenderable : IRenderable
{
	private ResourceSet viewInfoSet;
	private Pipeline starfieldPipeline;

	public static StarfieldRenderable Create(DeviceBuffer viewInfoBuffer, GraphicsWorld graphicsWorld)
	{
		StarfieldRenderable starfieldRenderable = new();

		starfieldRenderable.CreateDeviceObjects(viewInfoBuffer, graphicsWorld);

		return starfieldRenderable;
	}

	private StarfieldRenderable()
	{ }

	public void CreateDeviceObjects(DeviceBuffer viewInfoBuffer, GraphicsWorld graphicsWorld)
	{
		ResourceLayout invCameraInfoLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("InvCameraInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

		viewInfoSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(invCameraInfoLayout, viewInfoBuffer));

		ShaderSetDescription starfieldShaders = new(
			[],
			Utils.LoadShaders(SerializationPaths.ShadersPath, "Starfield", graphicsWorld.Factory));

		starfieldPipeline = graphicsWorld.Factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
			BlendStateDescription.SingleOverrideBlend,
			DepthStencilStateDescription.Disabled,
			RasterizerStateDescription.CullNone,
			PrimitiveTopology.TriangleList,
			starfieldShaders,
			[invCameraInfoLayout],
			graphicsWorld.RenderingOutputDescription));
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 _)
	{
		commandList.SetPipeline(starfieldPipeline);
		commandList.SetGraphicsResourceSet(0, viewInfoSet);
		commandList.Draw(4);
	}
}

public struct MatrixPair
{
	public Matrix4x4 First;
	public Matrix4x4 Second;

	public MatrixPair(Matrix4x4 first, Matrix4x4 second)
	{
		First = first;
		Second = second;
	}
}
