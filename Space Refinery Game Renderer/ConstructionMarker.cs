using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game_Renderer
{
	public sealed class ConstructionMarker : IRenderable
	{
		public enum ConstructionMarkerState
		{
			LegalBuild,
			IllegalBuild,
		}

		public ConstructionMarkerState State
		{
			get => state;
			set
			{
				switch (value)
				{
					case ConstructionMarkerState.LegalBuild:
						SetColor(RgbaFloat.Green);
						break;
					case ConstructionMarkerState.IllegalBuild:
						SetColor(RgbaFloat.Red);
						break;
				}

				state = value;
			}
		}

		public static ConstructionMarker Create(GraphicsWorld graphicsWorld)
		{
			DeviceBuffer transformationBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));

			DeviceBuffer colorBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription((uint)RgbaFloat.SizeInBytes, BufferUsage.VertexBuffer));

			ConstructionMarker constructionMarker = new(graphicsWorld, transformationBuffer, colorBuffer);

			CreateDeviceObjects(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, graphicsWorld);

			graphicsWorld.AddRenderable(constructionMarker);

			return constructionMarker;
		}

		private ConstructionMarker(GraphicsWorld gw, DeviceBuffer transformationBuffer, DeviceBuffer colorBuffer)
		{
			graphicsWorld = gw;
			this.transformationBuffer = transformationBuffer;
			this.colorBuffer = colorBuffer;

			state = ConstructionMarkerState.IllegalBuild;
		}

		// Normally RenderingResources would be used for device resources, however several layouts are different and therefore cannot use (at least not without modifying the shader) RenderingResources.
		private static void CreateDeviceObjects(GraphicsDevice gd, ResourceFactory factory, GraphicsWorld graphicsWorld)
		{
			if (hasDeviceResources)
			{
				return;
			}

			ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
			{
				new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
			};
			ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
			sharedLayout = factory.CreateResourceLayout(resourceLayoutDescription);

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
					vertexLayouts: new VertexLayoutDescription[] { RenderingResources.VertexLayout, colorVertexLayout, RenderingResources.TransformationVertexShaderParameterLayout },
					shaders: Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "DebugRenderable", factory)
				),
				Outputs = gd.MainSwapchain.Framebuffer.OutputDescription
			};

			pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

			BindableResource[] bindableResources = new BindableResource[] { graphicsWorld.CameraProjViewBuffer };
			ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
			resourceSet = factory.CreateResourceSet(resourceSetDescription);

			hasDeviceResources = true;
		}

		private static bool hasDeviceResources;

		private static Pipeline pipeline;

		private static ResourceLayout sharedLayout;

		private static ResourceSet resourceSet;

		private DeviceBuffer transformationBuffer;

		private DeviceBuffer colorBuffer;

		public bool ShouldDraw;

		private GraphicsWorld graphicsWorld;

		private Mesh mesh;

		private ConstructionMarkerState state;


		public void SetTransform(Transform transform)
		{
			graphicsWorld.GraphicsDevice.UpdateBuffer(transformationBuffer, 0, transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));
		}

		public void SetColor(RgbaFloat color)
		{
			graphicsWorld.GraphicsDevice.UpdateBuffer(colorBuffer, 0, color);
		}

		public void SetMesh(Mesh mesh)
		{
			this.mesh = mesh;
		}

		public void AddDrawCommands(CommandList cl, FixedDecimalLong8 deltaTime)
		{
			if (!ShouldDraw)
				return;

			cl.SetPipeline(pipeline);
			cl.SetGraphicsResourceSet(0, resourceSet);

			cl.SetVertexBuffer(0, mesh.VertexBuffer);
			cl.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
			cl.SetVertexBuffer(1, colorBuffer);
			cl.SetVertexBuffer(2, transformationBuffer);

			cl.DrawIndexed(mesh.IndexCount);
		}

		public void Destroy()
		{
			graphicsWorld.RemoveRenderable(this);
		}
	}
}
