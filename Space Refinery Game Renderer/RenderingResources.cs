using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public static class RenderingResources
{
	public static bool HasCreatedStaticDeviceResources { get; private set; } = false;

	public static ResourceLayout PBRDataLayout { get; private set; }

	public static ResourceLayout AuxillaryDataLayout { get; private set; }

	public static ResourceLayout TextureLayout { get; private set; }

	public static ResourceLayout SharedLayout { get; private set; }

	public static VertexLayoutDescription TransformationVertexShaderParameterLayout { get; private set; }

	public static VertexLayoutDescription VertexLayout { get; private set; }

	public static Pipeline PipelineResource { get; private set; }

	public static void CreateStaticDeviceResources(GraphicsWorld graphicsWorld)
	{
		if (HasCreatedStaticDeviceResources)
		{
			return;
		}

		ResourceLayoutElementDescription[] pbrLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("PBRData", ResourceKind.UniformBuffer, ShaderStages.Fragment),
		};
		PBRDataLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(pbrLayoutDescriptions));

		ResourceLayoutElementDescription[] textureLayoutDescriptions =
		{
			new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment),
		};
		TextureLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(textureLayoutDescriptions));

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

		GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription()
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
			ResourceLayouts = new ResourceLayout[] { SharedLayout, TextureLayout, PBRDataLayout, AuxillaryDataLayout },
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: new VertexLayoutDescription[] { VertexLayout, TransformationVertexShaderParameterLayout },
				shaders: graphicsWorld.ShaderLoader.LoadCached("EntityRenderable")
			),
			Outputs = graphicsWorld.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription
		};

		PipelineResource = graphicsWorld.Factory.CreateGraphicsPipeline(pipelineDescription);

		HasCreatedStaticDeviceResources = true;
	}
}
