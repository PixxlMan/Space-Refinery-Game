using FixedPrecision;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public class StarfieldRenderable : IRenderable
{
	private ResourceSet viewInfoSet;
	private Pipeline starfieldPipeline;

	public static StarfieldRenderable Create(DeviceBuffer viewInfoBuffer, GraphicsDevice gd, ResourceFactory factory)
	{
		StarfieldRenderable starfieldRenderable = new();

		starfieldRenderable.CreateDeviceObjects(viewInfoBuffer, gd, factory);

		return starfieldRenderable;
	}

	private StarfieldRenderable()
	{ }

	public void CreateDeviceObjects(DeviceBuffer viewInfoBuffer, GraphicsDevice gd, ResourceFactory factory)
	{
		ResourceLayout invCameraInfoLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("InvCameraInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

		viewInfoSet = factory.CreateResourceSet(new ResourceSetDescription(invCameraInfoLayout, viewInfoBuffer));

		ShaderSetDescription starfieldShaders = new ShaderSetDescription(
			Array.Empty<VertexLayoutDescription>(),
			Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "Starfield", factory));

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
