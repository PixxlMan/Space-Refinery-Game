using FixedPrecision;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public class SkyRenderable : IRenderable
{
	private ResourceSet viewInfoSet;
	private Pipeline starfieldPipeline;

	public static SkyRenderable Create(DeviceBuffer viewInfoBuffer, GraphicsDevice gd, ResourceFactory factory)
	{
		SkyRenderable skyRenderable = new();

		skyRenderable.CreateDeviceObjects(viewInfoBuffer, gd, factory);

		return skyRenderable;
	}

	private SkyRenderable()
	{ }

	public void CreateDeviceObjects(DeviceBuffer viewInfoBuffer, GraphicsDevice gd, ResourceFactory factory)
	{
		ResourceLayout invCameraInfoLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("InvCameraInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

		viewInfoSet = factory.CreateResourceSet(new ResourceSetDescription(invCameraInfoLayout, viewInfoBuffer));

		ShaderSetDescription starfieldShaders = new(
			[],
			Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "Sky", factory));

		starfieldPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
			BlendStateDescription.SingleOverrideBlend,
			DepthStencilStateDescription.Disabled,
			RasterizerStateDescription.CullNone,
			PrimitiveTopology.TriangleList,
			starfieldShaders,
			[invCameraInfoLayout],
			gd.MainSwapchain.Framebuffer.OutputDescription));
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 _)
	{
		commandList.SetPipeline(starfieldPipeline);
		commandList.SetGraphicsResourceSet(0, viewInfoSet);
		commandList.Draw(4);
	}
}
