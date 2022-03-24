using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FixedPrecision;
using FXRenderer;
using Veldrid;

namespace Space_Refinery_Game_Renderer
{
	public class DebugRender
	{
		private Pipeline pipeline;

		private ResourceLayout sharedLayout;

		private ResourceSet resourceSet;

		private List<DebugRenderable> debugRenderables = new();

		public static DebugRender Create(GraphicsWorld graphicsWorld)
		{
			DebugRender debugRender = new(graphicsWorld);

			graphicsWorld.CustomDrawOperations += debugRender.DrawDebugObjects;

			debugRender.CreateDeviceObjects(graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

			return debugRender;
		}

		private void CreateDeviceObjects(GraphicsDevice gd, ResourceFactory factory)
		{
			ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
			{
				new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
			};
			ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
			sharedLayout = factory.CreateResourceLayout(resourceLayoutDescription);


			VertexLayoutDescription transformationVertexShaderParameterLayout = new VertexLayoutDescription(
				new VertexElementDescription("InstancePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("InstanceRotationM11", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM12", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM13", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM21", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM22", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM23", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM31", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM32", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceRotationM33", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
				new VertexElementDescription("InstanceScale", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
				);
			transformationVertexShaderParameterLayout.InstanceStepRate = 1;

			VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
				new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

			VertexLayoutDescription colorVertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
			colorVertexLayout.InstanceStepRate = 1;

			GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription()
			{
				BlendState = BlendStateDescription.SingleOverrideBlend,
				DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual),
				RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.None,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false
				),
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				ResourceLayouts = new ResourceLayout[] { sharedLayout },
				ShaderSet = new ShaderSetDescription(
					vertexLayouts: new VertexLayoutDescription[] { vertexLayout, colorVertexLayout, transformationVertexShaderParameterLayout },
					shaders: Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "DebugRenderable", factory)
				),
				Outputs = gd.MainSwapchain.Framebuffer.OutputDescription
			};

			pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

			BindableResource[] bindableResources = new BindableResource[] { GraphicsWorld.CameraProjViewBuffer };
			ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
			resourceSet = factory.CreateResourceSet(resourceSetDescription);
		}

		private void DrawDebugObjects(CommandList cl)
		{
			cl.SetPipeline(pipeline);
			cl.SetGraphicsResourceSet(0, resourceSet);

			foreach (var renderable in debugRenderables)
			{
				renderable.AddDrawCommands(cl);
			}
		}

		private DebugRender(GraphicsWorld graphicsWorld)
		{
			GraphicsWorld = graphicsWorld;
		}

		public GraphicsWorld GraphicsWorld;

		public void DrawCube(Transform transform, RgbaFloat color)
		{
			var renderable = DebugRenderable.Create(Utils.CreateDeviceResources(Utils.GetCubeVertexPositionTexture(Vector3.One), Utils.GetCubeIndices(), GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory), transform, color, GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory);

			debugRenderables.Add(renderable);
		}

		public void DrawRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, RgbaFloat color)
		{
			Transform transform = new(origin, QuaternionFixedDecimalInt4.CreateLookingAt(direction));

			var renderable = DebugRenderable.Create(Utils.CreateDeviceResources(Utils.GetCubeVertexPositionTexture(new(.1f, .1f, 2f)), Utils.GetCubeIndices(), GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory), transform, color, GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory);

			debugRenderables.Add(renderable);
		}
	}
}
