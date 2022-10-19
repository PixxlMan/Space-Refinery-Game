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

		public static ConstructionMarker Create(GraphicsWorld gw)
		{
			DeviceBuffer transformationBuffer = gw.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));

			DeviceBuffer colorBuffer = gw.Factory.CreateBuffer(new BufferDescription((uint)RgbaFloat.SizeInBytes, BufferUsage.VertexBuffer));

			ConstructionMarker constructionMarker = new(gw, transformationBuffer, colorBuffer);

			constructionMarker.CreateDeviceObjects(gw.GraphicsDevice, gw.Factory);

			gw.AddRenderable(constructionMarker);

			return constructionMarker;
		}

		private ConstructionMarker(GraphicsWorld gw, DeviceBuffer transformationBuffer, DeviceBuffer colorBuffer)
		{
			graphicsWorld = gw;
			this.transformationBuffer = transformationBuffer;
			this.colorBuffer = colorBuffer;

			state = ConstructionMarkerState.IllegalBuild;
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

			BindableResource[] bindableResources = new BindableResource[] { graphicsWorld.CameraProjViewBuffer };
			ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
			resourceSet = factory.CreateResourceSet(resourceSetDescription);
		}

		private Pipeline pipeline;

		private ResourceLayout sharedLayout;

		private ResourceSet resourceSet;

		public bool ShouldDraw;

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

		private GraphicsWorld graphicsWorld;

		private Mesh mesh;

		private DeviceBuffer transformationBuffer;

		private DeviceBuffer colorBuffer;

		private ConstructionMarkerState state;

		public void AddDrawCommands(CommandList cl)
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
