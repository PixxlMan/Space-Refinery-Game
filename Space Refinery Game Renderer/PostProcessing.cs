using FixedPrecision;
using Space_Refinery_Utilities;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public sealed class PostProcessing
{
	private SortedDictionary<int, List<IPostEffect>> postEffects = [];

	private Texture screenTextureColorIn;
	public Texture screenTextureColorOut;
	
	private TextureView screenTextureColorInView;
	private TextureView screenTextureDepthView;

	private GraphicsWorld graphicsWorld;

	public void CreateDeviceResources(GraphicsWorld graphicsWorld)
	{
		this.graphicsWorld = graphicsWorld;

		var color = this.graphicsWorld.RenderFramebuffer.ColorTargets[0].Target;

		screenTextureColorIn = this.graphicsWorld.Factory.CreateTexture(TextureDescription.Texture2D(color.Width, color.Height, 1, 1, color.Format, TextureUsage.Sampled));
		screenTextureColorIn.Name = "Screen Texture Color In";
		screenTextureColorOut = this.graphicsWorld.Factory.CreateTexture(TextureDescription.Texture2D(color.Width, color.Height, 1, 1, color.Format, TextureUsage.Sampled | TextureUsage.Storage | TextureUsage.RenderTarget));
		screenTextureColorOut.Name = "Screen Texture Color Out";

		screenTextureColorInView = this.graphicsWorld.Factory.CreateTextureView(screenTextureColorIn);
		screenTextureColorInView.Name = "Screen Texture Color In View";
		screenTextureDepthView = this.graphicsWorld.Factory.CreateTextureView(graphicsWorld.RenderFramebuffer.DepthTarget!.Value.Target);
		screenTextureDepthView.Name = "Screen Depth View";
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

		postEffect.CreateDeviceObjects(graphicsWorld, screenTextureColorInView, screenTextureColorOut, screenTextureDepthView);

		Logging.LogScopeEnd();
	}

	public void AddPostEffectCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.PushDebugGroup("Post processing effects");

		commandList.CopyTexture(graphicsWorld.RenderFramebuffer.ColorTargets[0].Target, screenTextureColorIn);

		foreach ((_, List<IPostEffect> currentOrderPostEffects) in postEffects)
		{
			foreach (IPostEffect postEffect in currentOrderPostEffects)
			{
				commandList.PushDebugGroup(postEffect.Name);

				postEffect.AddEffectCommands(commandList, deltaTime);

				commandList.CopyTexture(screenTextureColorOut, screenTextureColorIn);

				commandList.PopDebugGroup();
			}
		}

		commandList.CopyTexture(screenTextureColorOut, graphicsWorld.RenderFramebuffer.ColorTargets[0].Target);

		commandList.PopDebugGroup();
	}
}

public interface IPostEffect
{
	public abstract string Name { get; }

	public void AddEffectCommands(CommandList commandList, FixedDecimalLong8 deltaTime);

	public void CreateDeviceObjects(GraphicsWorld graphicsWorld, TextureView screenTextureColorInView, Texture screenTextureColorOut, TextureView screenTextureDepthView);
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

		commandList.Dispatch((screenTextureColorInView.Target.Width + 31) / 32, (screenTextureColorInView.Target.Height + 31) / 32, 1);
	}

	public void CreateDeviceObjects(GraphicsWorld graphicsWorld, TextureView screenTextureColorInView, Texture screenTextureColorOut, TextureView screenTextureDepthView)
	{
		if (hasCreatedDeviceObjects)
		{
			return;
		}

		this.graphicsWorld = graphicsWorld;

		this.screenTextureColorInView = screenTextureColorInView;
		this.screenTextureColorOut = screenTextureColorOut;
		this.screenTextureDepthView = screenTextureDepthView;

		ResourceLayoutDescription bloomResourceLayoutDescription = new(
			new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorIn", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureDepth", ResourceKind.TextureReadOnly, ShaderStages.Compute),
			new ResourceLayoutElementDescription("ScreenTextureColorOut", ResourceKind.TextureReadWrite, ShaderStages.Compute));
		ResourceLayout bloomResourceLayout = graphicsWorld.Factory.CreateResourceLayout(ref bloomResourceLayoutDescription);

		ResourceSetDescription bloomResourcesDescription = new(
			bloomResourceLayout,
			[
				graphicsWorld.GraphicsDevice.PointSampler,
				screenTextureColorInView,
				screenTextureDepthView,
				screenTextureColorOut,
			]);
		bloomResources = graphicsWorld.Factory.CreateResourceSet(ref bloomResourcesDescription);

		ComputePipelineDescription postProcessingPipelineDescription = new()
		{
			//Specializations = [new SpecializationConstant()],
			ComputeShader = graphicsWorld.ShaderLoader.LoadComputeCached("Bloom"),
			//ThreadGroupSizeX = 32,
			//ThreadGroupSizeY = 32,
			//ThreadGroupSizeZ = 1,
			ResourceLayouts = [bloomResourceLayout],
		};
		bloomPipeline = graphicsWorld.Factory.CreateComputePipeline(ref postProcessingPipelineDescription);

		hasCreatedDeviceObjects = true;
	}
}
