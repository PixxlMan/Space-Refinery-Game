using FixedPrecision;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public class SkyRenderable : IRenderable
{
	private ResourceSet viewInfoSet;
	private Pipeline starfieldPipeline;

	public static SkyRenderable Create(DeviceBuffer viewInfoBuffer, GraphicsWorld graphicsWorld)
	{
		SkyRenderable skyRenderable = new();

		skyRenderable.CreateDeviceObjects(viewInfoBuffer, graphicsWorld);

		return skyRenderable;
	}

	private SkyRenderable()
	{ }

	public void CreateDeviceObjects(DeviceBuffer viewInfoBuffer, GraphicsWorld graphicsWorld)
	{
		ResourceLayout invCameraInfoLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("InvCameraInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

		viewInfoSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(invCameraInfoLayout, viewInfoBuffer));

		ShaderSetDescription starfieldShaders = new(
			[],
			Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "Sky", graphicsWorld.Factory));

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
