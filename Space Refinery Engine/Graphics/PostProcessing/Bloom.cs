using FixedPrecision;
using Veldrid;

namespace Space_Refinery_Game.Renderer;

public sealed record class Bloom : IPostEffect
{
	public string Name => "Bloom";

	private static bool hasCreatedDeviceObjects = false;

	private static Pipeline bloomThresholdPipeline;
	private static Pipeline bloomBlurPipeline;

	private static ResourceSet bloomThresholdResources;
	private static ResourceSet bloomBlurResources;

	private DeviceBuffer passInfoBuffer;

	private TextureView screenTextureColorInView;
	private TextureView thresholdTextureView;
	private Texture screenTextureColorOut;

	private GraphicsWorld graphicsWorld;

	public float Threshold { get; set; }

	public float Intensity { get; set; }

	private int firstPass = 0;
	private int secondPass = 1;

	public void AddEffectCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.SetPipeline(bloomThresholdPipeline);
		commandList.SetComputeResourceSet(0, bloomThresholdResources);
		commandList.Dispatch((screenTextureColorInView.Target.Width + 31) / 32, (screenTextureColorInView.Target.Height + 31) / 32, 1);

		commandList.CopyTexture(screenTextureColorOut, thresholdTextureView.Target);
		commandList.SetPipeline(bloomBlurPipeline);

		commandList.SetComputeResourceSet(0, bloomBlurResources);

		commandList.UpdateBuffer(passInfoBuffer, 0, ref firstPass, sizeof(int));
		commandList.Dispatch((screenTextureColorInView.Target.Width + 31) / 32, (screenTextureColorInView.Target.Height + 31) / 32, 1);

		commandList.CopyTexture(screenTextureColorOut, thresholdTextureView.Target);

		commandList.UpdateBuffer(passInfoBuffer, 0, ref secondPass, sizeof(int));
		commandList.Dispatch((screenTextureColorInView.Target.Width + 31) / 32, (screenTextureColorInView.Target.Height + 31) / 32, 1);
	}

	public void CreateDeviceObjects(GraphicsWorld graphicsWorld, TextureView screenTextureColorInView, Texture screenTextureColorOut, TextureView _)
	{
		if (hasCreatedDeviceObjects)
		{
			return;
		}

		this.graphicsWorld = graphicsWorld;

		this.screenTextureColorInView = screenTextureColorInView;
		this.screenTextureColorOut = screenTextureColorOut;

		thresholdTextureView = graphicsWorld.Factory.CreateTextureView(Utils.CloneTexture(screenTextureColorOut, graphicsWorld.GraphicsDevice, graphicsWorld.Factory));


		ResourceLayoutDescription bloomThresholdResourceLayoutDescription = new(
			new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorIn", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorOut", ResourceKind.TextureReadWrite, ShaderStages.Compute));
		ResourceLayout bloomThresholdResourceLayout = graphicsWorld.Factory.CreateResourceLayout(ref bloomThresholdResourceLayoutDescription);

		ResourceSetDescription bloomThresholdResourcesDescription = new(
			bloomThresholdResourceLayout,
			[
				graphicsWorld.GraphicsDevice.PointSampler,
				screenTextureColorInView,
				screenTextureColorOut,
			]);
		bloomThresholdResources = graphicsWorld.Factory.CreateResourceSet(ref bloomThresholdResourcesDescription);


		passInfoBuffer = graphicsWorld.Factory.CreateBuffer(new(16, BufferUsage.Dynamic | BufferUsage.UniformBuffer));

		ResourceLayoutDescription bloomBlurResourceLayoutDescription = new(
			new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ThresholdIn", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorIn", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorOut", ResourceKind.TextureReadWrite, ShaderStages.Compute),
			new ResourceLayoutElementDescription("PassInfo", ResourceKind.UniformBuffer, ShaderStages.Compute));
		ResourceLayout bloomBlurResourceLayout = graphicsWorld.Factory.CreateResourceLayout(ref bloomBlurResourceLayoutDescription);

		ResourceSetDescription bloomBlurResourcesDescription = new(
			bloomBlurResourceLayout,
			[
				graphicsWorld.GraphicsDevice.PointSampler,
				thresholdTextureView,
				screenTextureColorInView,
				screenTextureColorOut,
				passInfoBuffer,
			]);
		bloomBlurResources = graphicsWorld.Factory.CreateResourceSet(ref bloomBlurResourcesDescription);


		ComputePipelineDescription bloomThresholdPipelineDescription = new()
		{
			ComputeShader = graphicsWorld.ShaderLoader.LoadComputeCached("Bloom-threshold"),
			ResourceLayouts = [bloomThresholdResourceLayout],
		};
		bloomThresholdPipeline = graphicsWorld.Factory.CreateComputePipeline(ref bloomThresholdPipelineDescription);

		ComputePipelineDescription bloomBlurPipelineDescription = new()
		{
			ComputeShader = graphicsWorld.ShaderLoader.LoadComputeCached("Bloom-blur"),
			ResourceLayouts = [bloomBlurResourceLayout],
		};
		bloomBlurPipeline = graphicsWorld.Factory.CreateComputePipeline(ref bloomBlurPipelineDescription);

		hasCreatedDeviceObjects = true;
	}
}
