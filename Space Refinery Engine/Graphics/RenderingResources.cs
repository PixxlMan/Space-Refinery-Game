using FixedPrecision;
using Space_Refinery_Engine;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Space_Refinery_Engine.Renderer;

public static class RenderingResources
{
	public static bool HasCreatedStaticDeviceResources { get; private set; } = false;

	public static Texture WhiteTexture { get; private set; }

	public static Texture NeutralNormalTexture { get; private set; }

	public static Texture DefaultTexture { get; private set; }
	public static Material DefaultMaterial { get; private set; }

	/// <summary>
	/// A simple fragment stage resource layout with one texture in the first location and a sampler in the second.
	/// </summary>
	public static ResourceLayout TextureLayout { get; private set; }
	public static ResourceLayout MaterialLayout { get; private set; }
	public static ResourceLayout SharedLayout { get; private set; }
	public static ResourceLayout CameraProjViewOnlyLayout { get; private set; }
	public static ResourceLayout ShadowRecieverLayout { get; private set; }
	public static ResourceLayout ShadowMapTextureLayout { get; private set; }

	public static VertexLayoutDescription TransformationVertexShaderParameterLayout { get; private set; }
	public static VertexLayoutDescription VertexLayout { get; private set; }

	public static Pipeline ClockwisePipelineResource { get; private set; }
	public static Pipeline CounterClockwisePipelineResource { get; private set; }

	public static Pipeline ShadowCasterPipelineResource { get; private set; }

	public static PixelFormat DepthFormat => PixelFormat.R16_UNorm;
	public static PixelFormat ColorFormat => PixelFormat.B8_G8_R8_A8_UNorm;
	public static PixelFormat InternalColorFormat => PixelFormat.R32_G32_B32_A32_Float;

	public static ReadOnlyMemory<VertexPositionTexture2D> FullscreenQuadVertexPositionTexture2D { get; private set; }
	public static ushort[] FullscreenQuadIndicies { get; } = [0, 1, 2, 0, 2, 3];
	public static DeviceBuffer FullscreenQuadVertexBuffer { get; private set; }
	public static DeviceBuffer FullscreenQuadIndexBuffer { get; private set; }
	public static VertexLayoutDescription FullscreenQuadVertexLayout { get; private set; }
	public static Pipeline FullscreenQuadPipeline { get; private set; }
	
	public static DeviceBuffer CameraProjViewBuffer { get; private set; }
	public static DeviceBuffer LightInfoBuffer { get; private set; }
	public static DeviceBuffer ViewInfoBuffer { get; private set; }

	public static ResourceSet SharedResourceSet { get; private set; }
	public static ResourceSet ShadowRecieverResourceSet { get; private set; }

	public static ResourceSet CameraProjViewOnlyResourceSet { get; private set; }

	public static ResourceSet ShadowCasterProjViewOnlyResourceSet { get; private set; }

	public static DeviceBuffer ShadowProjViewBuffer { get; private set; }

	public static DeviceBuffer ShadowRecieverBuffer { get; private set; }

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

		ResourceLayoutElementDescription[] shadowMapResourceLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
		};
		ShadowMapTextureLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(shadowMapResourceLayoutDescriptions));

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

		ResourceLayoutElementDescription[] shadowRecieverResourceLayourElementDescriptions =
		{
			new ResourceLayoutElementDescription("ShadowInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
		};
		ResourceLayoutDescription shadowRecieverResourceLayoutDescription = new ResourceLayoutDescription(shadowRecieverResourceLayourElementDescriptions);
		ShadowRecieverLayout = graphicsWorld.Factory.CreateResourceLayout(ref shadowRecieverResourceLayoutDescription);

		ResourceLayoutElementDescription[] cameraProjViewOnlyLayoutElementDescription =
		{
			new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
		};
		ResourceLayoutDescription cameraProjViewOnlyLayoutDescription = new ResourceLayoutDescription(cameraProjViewOnlyLayoutElementDescription);
		CameraProjViewOnlyLayout = graphicsWorld.Factory.CreateResourceLayout(ref cameraProjViewOnlyLayoutDescription);

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
			new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("Tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
			);

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
			ResourceLayouts = [SharedLayout, MaterialLayout, ShadowRecieverLayout, ShadowMapTextureLayout],
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
			ResourceLayouts = [SharedLayout, MaterialLayout, ShadowRecieverLayout, ShadowMapTextureLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [VertexLayout, TransformationVertexShaderParameterLayout],
				shaders: graphicsWorld.ShaderLoader.LoadVertexFragmentCached("EntityRenderable")
			),
			Outputs = graphicsWorld.RenderingOutputDescription
		};
		CounterClockwisePipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(ref counterClockwisePipelineDescription);

		GraphicsPipelineDescription shadowCasterPipelineDescription = new()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual
			),
			RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Front, // If shadows are cast only from backfaces, we can avoid shadow acne.
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false
			),
			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = [CameraProjViewOnlyLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [VertexLayout, TransformationVertexShaderParameterLayout],
				shaders: graphicsWorld.ShaderLoader.LoadVertexFragmentCached("ShadowCaster")
			),
			Outputs = graphicsWorld.ShadowRenderingOutputDescription
		};
		ShadowCasterPipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(ref shadowCasterPipelineDescription);

		FullscreenQuadVertexPositionTexture2D = Utils.GetQuadVertexPositionTexture();

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


		CameraProjViewBuffer = graphicsWorld.Factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		LightInfoBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		ViewInfoBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MatrixPair>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		
		ShadowProjViewBuffer = graphicsWorld.Factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		
		ShadowRecieverBuffer = graphicsWorld.Factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>()), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

		ResourceSetDescription sharedResourceSetDescription = new(SharedLayout, [LightInfoBuffer, CameraProjViewBuffer]);
		SharedResourceSet = graphicsWorld.Factory.CreateResourceSet(ref sharedResourceSetDescription);

		ResourceSetDescription shadowRecieverResourceSetDescription = new(ShadowRecieverLayout, [ShadowRecieverBuffer]);
		ShadowRecieverResourceSet = graphicsWorld.Factory.CreateResourceSet(ref shadowRecieverResourceSetDescription);

		ResourceSetDescription cameraProjViewOnlyResourceSetDescription = new(CameraProjViewOnlyLayout, [CameraProjViewBuffer]);
		CameraProjViewOnlyResourceSet = graphicsWorld.Factory.CreateResourceSet(ref cameraProjViewOnlyResourceSetDescription);

		ResourceSetDescription shadowCasterProjViewOnlyResourceSetDescription = new(CameraProjViewOnlyLayout, [ShadowProjViewBuffer]);
		ShadowCasterProjViewOnlyResourceSet = graphicsWorld.Factory.CreateResourceSet(ref shadowCasterProjViewOnlyResourceSetDescription);

		WhiteTexture = Utils.GetSolidColoredTexture(RgbaByte.White, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		NeutralNormalTexture = Utils.GetSolidColoredTexture(new RgbaByte(128, 128, 255, 1), graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		DefaultTexture = Utils.GetSolidColoredTexture(RgbaByte.LightGrey, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
		DefaultMaterial = Material.FromTextures("Default Material", DefaultTexture, NeutralNormalTexture, Utils.GetSolidColoredTexture(RgbaByte.Black, graphicsWorld.GraphicsDevice, graphicsWorld.Factory), DefaultTexture, DefaultTexture, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		HasCreatedStaticDeviceResources = true;

		Logging.LogScopeEnd();
	}
}
