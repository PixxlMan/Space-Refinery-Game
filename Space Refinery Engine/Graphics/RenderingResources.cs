using Space_Refinery_Engine;
using Veldrid;

namespace Space_Refinery_Engine.Renderer;

public static class RenderingResources
{
	public static bool HasCreatedStaticDeviceResources { get; private set; } = false;

	public static Texture WhiteTexture { get; private set; }

	public static Texture NeutralNormal { get; private set; }

	public static Texture DefaultTexture { get; private set; }
	public static Material DefaultMaterial { get; private set; }

	/// <summary>
	/// A simple fragment stage resource layout with one texture in the first location and a sampler in the second.
	/// </summary>
	public static ResourceLayout TextureLayout { get; private set; }
	public static ResourceLayout MaterialLayout { get; private set; }
	public static ResourceLayout SharedLayout { get; private set; }

	public static VertexLayoutDescription TransformationVertexShaderParameterLayout { get; private set; }
	public static VertexLayoutDescription VertexLayout { get; private set; }

	public static Pipeline ClockwisePipelineResource { get; private set; }
	public static Pipeline CounterClockwisePipelineResource { get; private set; }

	public static PixelFormat DepthFormat => PixelFormat.R16_UNorm;
	public static PixelFormat ColorFormat => PixelFormat.B8_G8_R8_A8_UNorm;
	public static PixelFormat InternalColorFormat => PixelFormat.R32_G32_B32_A32_Float;

	public static ReadOnlyMemory<VertexPositionTexture2D> FullscreenQuadVertexPositionTexture2D { get; } = Utils.GetQuadVertexPositionTexture();
	public static ushort[] FullscreenQuadIndicies { get; } = [0, 1, 2, 0, 2, 3];
	public static DeviceBuffer FullscreenQuadVertexBuffer { get; private set; }
	public static DeviceBuffer FullscreenQuadIndexBuffer { get; private set; }
	public static VertexLayoutDescription FullscreenQuadVertexLayout { get; private set; }
	public static Pipeline FullscreenQuadPipeline { get; private set; } 

	public static void CreateStaticDeviceResources(GraphicsWorld graphicsWorld)
	{
		if (HasCreatedStaticDeviceResources)
		{
			return;
		}

		Logging.LogScopeStart("Creating rendering resources");

		ResourceLayoutElementDescription[] textureLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment),
		};
		TextureLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(textureLayoutDescriptions));

		ResourceLayoutElementDescription[] materialLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("AlbedoTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("NormalTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("MetallicTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("RoughnessTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("AmbientOcclusionTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
		};
		MaterialLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(materialLayoutDescriptions));

		ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
		{
			new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
		};
		ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
		SharedLayout = graphicsWorld.Factory.CreateResourceLayout(ref resourceLayoutDescription);

		TransformationVertexShaderParameterLayout = new VertexLayoutDescription(
				new VertexElementDescription("InstancePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("InstanceRotationM11", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM12", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM13", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM21", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM22", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM23", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM31", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM32", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM33", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)
			)
		{ InstanceStepRate = 1 };

		VertexLayout = new VertexLayoutDescription(
			new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
			new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
			new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

		GraphicsPipelineDescription clockwisePipelineDescription = new()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual
			),
			RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Back,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false
			),
			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = [SharedLayout, MaterialLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [VertexLayout, TransformationVertexShaderParameterLayout],
				shaders: graphicsWorld.ShaderLoader.LoadVertexFragmentCached("EntityRenderable")
			),
			Outputs = graphicsWorld.RenderingOutputDescription
		};
		ClockwisePipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(ref clockwisePipelineDescription);

		GraphicsPipelineDescription counterClockwisePipelineDescription = new()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual
			),
			RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Back,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.CounterClockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false
			),
			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = [SharedLayout, MaterialLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [VertexLayout, TransformationVertexShaderParameterLayout],
				shaders: graphicsWorld.ShaderLoader.LoadVertexFragmentCached("EntityRenderable")
			),
			Outputs = graphicsWorld.RenderingOutputDescription
		};
		CounterClockwisePipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(ref counterClockwisePipelineDescription);

		FullscreenQuadVertexLayout = new(
			new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
			new VertexElementDescription("TexCoord", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
		);

		BufferDescription fullscreenQuadVertexBufferDescription = new(VertexPositionTexture2D.SizeInBytes * (uint)FullscreenQuadVertexPositionTexture2D.Length, BufferUsage.VertexBuffer);
		FullscreenQuadVertexBuffer = graphicsWorld.Factory.CreateBuffer(ref fullscreenQuadVertexBufferDescription);
		graphicsWorld.GraphicsDevice.UpdateBuffer(FullscreenQuadVertexBuffer, 0, FullscreenQuadVertexPositionTexture2D.Span);

		BufferDescription fullscreenQuadIndexBufferDescription = new(sizeof(ushort) * (uint)FullscreenQuadIndicies.Length, BufferUsage.IndexBuffer);
		FullscreenQuadIndexBuffer = graphicsWorld.Factory.CreateBuffer(ref fullscreenQuadIndexBufferDescription);
		graphicsWorld.GraphicsDevice.UpdateBuffer(FullscreenQuadIndexBuffer, 0, FullscreenQuadIndicies);

		GraphicsPipelineDescription fullscreenQuadPipelineDescription = new()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: false,
				depthWriteEnabled: false,
				comparisonKind: ComparisonKind.LessEqual
			),
			RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.None,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: false,
				scissorTestEnabled: false
			),
			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = [TextureLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [FullscreenQuadVertexLayout],
				shaders: graphicsWorld.ShaderLoader.LoadVertexFragmentCached("FullscreenQuad")
			),
			Outputs = graphicsWorld.Swapchain.Framebuffer.OutputDescription
		};
		FullscreenQuadPipeline = graphicsWorld.Factory.CreateGraphicsPipeline(ref fullscreenQuadPipelineDescription);

		WhiteTexture = Utils.GetSolidColoredTexture(RgbaByte.White, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		NeutralNormal = Utils.GetSolidColoredTexture(new RgbaByte(128, 128, 255, 1), graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		DefaultTexture = Utils.GetSolidColoredTexture(RgbaByte.LightGrey, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
		DefaultMaterial = Material.FromTextures("Default Material", DefaultTexture, DefaultTexture, DefaultTexture, DefaultTexture, DefaultTexture, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		HasCreatedStaticDeviceResources = true;

		Logging.LogScopeEnd();
	}
}
