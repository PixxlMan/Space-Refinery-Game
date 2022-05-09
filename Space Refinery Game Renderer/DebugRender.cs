using FixedPrecision;
using FXRenderer;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game_Renderer
{
	public class DebugRender
	{
		public bool ShouldRender;

		private Pipeline pipeline;

		private ResourceLayout sharedLayout;

		private ResourceSet resourceSet;

		private Dictionary<Transform, DeviceBuffer> transformationBuffers = new();

		private Dictionary<Vector4, DeviceBuffer> colorBuffers = new();

		private Dictionary<Vector3FixedDecimalInt4, Mesh> cubeMeshes = new();

		private List<DebugRenderable> debugRenderables = new();

		private List<DebugRenderable> persistentRenderables = new();

		public event Action AddDebugObjects;

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
			if (!ShouldRender)
			{
				persistentRenderables.Clear();

				return;
			}

			AddDebugObjects?.Invoke();

			cl.PushDebugGroup("Debug objects");

			cl.SetPipeline(pipeline);
			cl.SetGraphicsResourceSet(0, resourceSet);

			foreach (var renderable in debugRenderables)
			{
				renderable.AddDrawCommands(cl);
				//renderable.Dispose();
			}

			foreach (var renderable in persistentRenderables)
			{
				renderable.AddDrawCommands(cl);
			}

			cl.PopDebugGroup();

			debugRenderables.Clear();
		}

		private DebugRender(GraphicsWorld graphicsWorld)
		{
			GraphicsWorld = graphicsWorld;
		}

		public GraphicsWorld GraphicsWorld;

		private void GetBuffers(RgbaFloat color, Transform transform, out DeviceBuffer transformationBuffer, out DeviceBuffer colorBuffer)
		{
			if (transformationBuffers.ContainsKey(transform))
			{
				transformationBuffer = transformationBuffers[transform];
			}
			else
			{
				transformationBuffer = GraphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));
				GraphicsWorld.GraphicsDevice.UpdateBuffer(transformationBuffer, 0, transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

				transformationBuffers.Add(transform, transformationBuffer);
			}

			if (colorBuffers.ContainsKey(color.ToVector4()))
			{
				colorBuffer = colorBuffers[color.ToVector4()];
			}
			else
			{
				colorBuffer = GraphicsWorld.Factory.CreateBuffer(new BufferDescription((uint)RgbaFloat.SizeInBytes, BufferUsage.VertexBuffer));
				GraphicsWorld.GraphicsDevice.UpdateBuffer(colorBuffer, 0, new Vector3(color.R, color.G, color.B));

				colorBuffers.Add(color.ToVector4(), colorBuffer);
			}
		}

		private Mesh GetCubeMesh(Vector3FixedDecimalInt4 size)
		{
			Mesh mesh;

			if (cubeMeshes.ContainsKey(size))
			{
				mesh = cubeMeshes[size];
			}
			else
			{
				mesh = Utils.CreateDeviceResources(Utils.GetCubeVertexPositionTexture(size.ToVector3()), Utils.GetCubeIndices(), GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory);

				cubeMeshes.Add(size, mesh);
			}

			return mesh;
		}


		// TODO: use separate size? instead of using transform.scale...
		public void DrawCube(Transform transform, RgbaFloat color)
		{
			DeviceBuffer transformationBuffer, colorBuffer;

			GetBuffers(color, new(transform) { Scale = Vector3FixedDecimalInt4.One }, out transformationBuffer, out colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(transform.Scale), transformationBuffer, colorBuffer);

			debugRenderables.Add(renderable);
		}

		public void PersistentCube(Transform transform, RgbaFloat color)
		{
			DeviceBuffer transformationBuffer, colorBuffer;

			GetBuffers(color, new(transform) { Scale = Vector3FixedDecimalInt4.One }, out transformationBuffer, out colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(transform.Scale), transformationBuffer, colorBuffer);

			persistentRenderables.Add(renderable);
		}

		public void DrawRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, RgbaFloat color) => DrawRay(origin, direction, 1, color);

		public void DrawRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 length, RgbaFloat color)
		{
			Transform transform = new(origin + (direction * length / 2), QuaternionFixedDecimalInt4.CreateLookingAt(direction, Vector3FixedDecimalInt4.UnitZ, Vector3FixedDecimalInt4.UnitY));

			DeviceBuffer transformationBuffer, colorBuffer;

			GetBuffers(color, transform, out transformationBuffer, out colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(new((FixedDecimalInt4).1, (FixedDecimalInt4).1, length)), transformationBuffer, colorBuffer);

			debugRenderables.Add(renderable);
		}

		public void PersistentRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, RgbaFloat color) => PersistentRay(origin, direction, 1, color);

		public void PersistentRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 length, RgbaFloat color)
		{
			Transform transform = new(origin, QuaternionFixedDecimalInt4.CreateLookingAt(direction, Vector3FixedDecimalInt4.UnitZ, Vector3FixedDecimalInt4.UnitY));

			DeviceBuffer transformationBuffer, colorBuffer;

			GetBuffers(color, transform, out transformationBuffer, out colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(new((FixedDecimalInt4).1, (FixedDecimalInt4).1, length)), transformationBuffer, colorBuffer);

			persistentRenderables.Add(renderable);
		}

		public void DrawOrientationMarks(Transform transform)
		{
			DrawRay(transform.Position, transform.LocalUnitX, RgbaFloat.Red);
			DrawRay(transform.Position, transform.LocalUnitY, RgbaFloat.Green);
			DrawRay(transform.Position, transform.LocalUnitZ, RgbaFloat.Blue);

			DrawRay(transform.Position, -transform.LocalUnitX, new(.4f, 0, 0, 1));
			DrawRay(transform.Position, -transform.LocalUnitY, new(0, .4f, 0, 1));
			DrawRay(transform.Position, -transform.LocalUnitZ, new(0, 0, .4f, 1));
		}
	}
}
