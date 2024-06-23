using FixedPrecision;
using Space_Refinery_Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public sealed class PostProcessing
{
	private SortedDictionary<int, List<IPostEffect>> postEffects = [];

	public Texture ScreenTextureColorIn;
	public Texture ScreenTextureColorOut;
	public Texture ScreenTextureDepth;
	
	public TextureView ScreenTextureColorInView;
	public TextureView ScreenTextureColorOutView;
	public TextureView ScreenTextureDepthView;

	private GraphicsWorld graphicsWorld;

	public void CreateDeviceResources(GraphicsWorld graphicsWorld)
	{
		this.graphicsWorld = graphicsWorld;

		var color = this.graphicsWorld.Swapchain.Framebuffer.ColorTargets[0].Target;
		var depth = this.graphicsWorld.Swapchain.Framebuffer.DepthTarget!.Value.Target;

		ScreenTextureColorIn = this.graphicsWorld.Factory.CreateTexture(new(color.Width, color.Height, 1, color.MipLevels, color.ArrayLayers, color.Format, TextureUsage.Sampled, TextureType.Texture2D, color.SampleCount));
		ScreenTextureColorOut = this.graphicsWorld.Factory.CreateTexture(new(color.Width, color.Height, 1, color.MipLevels, color.ArrayLayers, color.Format, TextureUsage.Storage, TextureType.Texture2D, color.SampleCount));
		ScreenTextureDepth = this.graphicsWorld.Factory.CreateTexture(new(depth.Width, depth.Height, 1, depth.MipLevels, depth.ArrayLayers, depth.Format, TextureUsage.Sampled, TextureType.Texture2D, depth.SampleCount));

		ScreenTextureColorInView = this.graphicsWorld.Factory.CreateTextureView(ScreenTextureColorIn);
		ScreenTextureDepthView = this.graphicsWorld.Factory.CreateTextureView(ScreenTextureDepth);
	}

	public void AddPostEffect(int order, IPostEffect postEffect)
	{
		Logging.LogScopeStart($"Adding post effect '{postEffect.Name}' at order position {order}");

		if (postEffects.TryGetValue(order, out var currentOrderPostEffects))
		{
			currentOrderPostEffects.Add(postEffect);
		}
		else
		{
			List<IPostEffect> list =
			[
				postEffect
			];
			postEffects.Add(order, list);
		}

		postEffect.CreateDeviceObjects(graphicsWorld, ScreenTextureColorInView, ScreenTextureColorOut, ScreenTextureDepthView);

		Logging.LogScopeEnd();
	}

	public void AddPostEffectCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.CopyTexture(graphicsWorld.Swapchain.Framebuffer.ColorTargets[0].Target, ScreenTextureColorIn);
		commandList.CopyTexture(graphicsWorld.Swapchain.Framebuffer.DepthTarget!.Value.Target, ScreenTextureDepth);

		Debug.Assert(graphicsWorld.Swapchain.Framebuffer.ColorTargets.Count == 1);

		foreach ((_, List<IPostEffect> currentOrderPostEffects) in postEffects)
		{
			foreach (IPostEffect postEffect in currentOrderPostEffects)
			{
				postEffect.AddEffectCommands(commandList, deltaTime);

				commandList.CopyTexture(ScreenTextureColorOut, ScreenTextureColorIn);
			}
		}

		commandList.CopyTexture(ScreenTextureColorIn, graphicsWorld.Swapchain.Framebuffer.ColorTargets[0].Target);
	}
}

public interface IPostEffect
{
	public abstract string Name { get; }

	public void AddEffectCommands(CommandList commandList, FixedDecimalLong8 deltaTime);

	public void CreateDeviceObjects(GraphicsWorld graphicsWorld, TextureView screenTextureColorIn, Texture screenTextureColorOut, TextureView screenTextureDepth);
}

public sealed record class Bloom : IPostEffect
{
	public string Name => "Bloom";

	private static bool hasCreatedDeviceObjects = false;

	private static Pipeline bloomPipeline;

	private static ResourceSet bloomResources;

	private TextureView screenTextureColorInView;
	private Texture screenTextureColorOut;
	private TextureView screenTextureDepthView;

	private GraphicsWorld graphicsWorld;

	public float Threshold { get; set; }

	public float Intensity { get; set; }

	public void AddEffectCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.SetPipeline(bloomPipeline);

		commandList.SetComputeResourceSet(0, bloomResources);

		commandList.Dispatch(screenTextureColorInView.Target.Width / 32, screenTextureColorInView.Target.Height / 32, 1);
	}

	public void CreateDeviceObjects(GraphicsWorld graphicsWorld, TextureView screenTextureColorIn, Texture screenTextureColorOut, TextureView screenTextureDepth)
	{
		if (hasCreatedDeviceObjects)
		{
			return;
		}

		this.graphicsWorld = graphicsWorld;

		this.screenTextureColorInView = screenTextureColorIn;
		this.screenTextureColorOut = screenTextureColorOut;
		this.screenTextureDepthView = screenTextureDepth;

		ResourceLayoutDescription bloomResourceLayoutDescription = new(
			new ResourceLayoutElementDescription("ScreenTextureColorIn", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorOut", ResourceKind.TextureReadWrite, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureDepth", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Compute));
		ResourceLayout bloomResourceLayout = graphicsWorld.Factory.CreateResourceLayout(bloomResourceLayoutDescription);

		ResourceSetDescription bloomResourcesDescription = new(bloomResourceLayout, screenTextureColorIn, screenTextureColorOut, screenTextureDepth, graphicsWorld.GraphicsDevice.PointSampler);
		bloomResources = graphicsWorld.Factory.CreateResourceSet(bloomResourcesDescription);

		ComputePipelineDescription postProcessingPipelineDescription = new()
		{
			//Specializations = [new SpecializationConstant()],
			ComputeShader = graphicsWorld.ShaderLoader.LoadComputeCached("Bloom"),
			ThreadGroupSizeX = 32,
			ThreadGroupSizeY = 32,
			ThreadGroupSizeZ = 1,
			ResourceLayouts = [bloomResourceLayout],
		};
		bloomPipeline = graphicsWorld.Factory.CreateComputePipeline(postProcessingPipelineDescription);

		hasCreatedDeviceObjects = true;
	}
}