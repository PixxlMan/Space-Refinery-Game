using Veldrid;

namespace Space_Refinery_Game_Renderer;

public static class RenderingResources
{
	public static Texture DefaultTexture { get; private set; }

	public static Material DefaultMaterial { get; private set; }

	public static bool HasCreatedStaticDeviceResources { get; private set; } = false;

	public static ResourceLayout TextureLayout { get; private set; }

	public static ResourceLayout MaterialLayout { get; private set; }

	public static ResourceLayout SharedLayout { get; private set; }

	public static VertexLayoutDescription TransformationVertexShaderParameterLayout { get; private set; }

	public static VertexLayoutDescription VertexLayout { get; private set; }

	public static Pipeline ClockwisePipelineResource { get; private set; }

	public static Pipeline CounterClockwisePipelineResource { get; private set; }

	public static void CreateStaticDeviceResources(GraphicsWorld graphicsWorld)
	{
		if (HasCreatedStaticDeviceResources)
		{
			return;
		}

		ResourceLayoutElementDescription[] textureLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment),
		};
		TextureLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(textureLayoutDescriptions));

		ResourceLayoutElementDescription[] materialLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("MetallicTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("RoughnessTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("AOTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
		};
		MaterialLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(materialLayoutDescriptions));

		ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
		{
			new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
		};
		ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
		SharedLayout = graphicsWorld.Factory.CreateResourceLayout(resourceLayoutDescription);

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
			comparisonKind: ComparisonKind.LessEqual),
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
				shaders: graphicsWorld.ShaderLoader.LoadCached("EntityRenderable")
			),
			Outputs = graphicsWorld.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription
		};

		ClockwisePipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(clockwisePipelineDescription);

		GraphicsPipelineDescription counterClockwisePipelineDescription = new()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
			depthTestEnabled: true,
			depthWriteEnabled: true,
			comparisonKind: ComparisonKind.LessEqual),
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
				shaders: graphicsWorld.ShaderLoader.LoadCached("EntityRenderable")
			),
			Outputs = graphicsWorld.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription
		};

		CounterClockwisePipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(counterClockwisePipelineDescription);

		DefaultTexture = Utils.GetSolidColoredTexture(RgbaByte.LightGrey, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
		DefaultMaterial = Material.FromTextures(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, "Default Material", DefaultTexture, DefaultTexture, DefaultTexture, DefaultTexture);

		HasCreatedStaticDeviceResources = true;
	}
}
