using FixedPrecision;
using Space_Refinery_Engine;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game.Renderer;

public sealed class DebugRender : IRenderable
{
	public bool ShouldRender { get; set; }

	public bool Wireframe { get; set; }

	private Pipeline regularPipeline;

	private Pipeline wireframePipeline;

	private ResourceLayout sharedLayout;

	private ResourceSet resourceSet;

	private Dictionary<Transform, DeviceBuffer> transformationBuffers = new();

	private Dictionary<Vector4, DeviceBuffer> colorBuffers = new();

	private Dictionary<Vector3FixedDecimalInt4, Mesh> cubeMeshes = new();

	private List<DebugRenderable> debugRenderables = new();

	private List<DebugRenderable> persistentRenderables = new();

	public event Action? AddDebugObjects;

	private object sync = new();

	public static DebugRender Create(GraphicsWorld graphicsWorld)
	{
		Logging.LogScopeStart("Creating DebugRender");

		DebugRender debugRender = new(graphicsWorld);

		graphicsWorld.AddRenderable(debugRender, 9_000);

		debugRender.CreateDeviceObjects(graphicsWorld);

		Logging.LogScopeEnd();
		return debugRender;
	}

	private void CreateDeviceObjects(GraphicsWorld graphicsWorld)
	{
		ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
		{
			new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
		};
		ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
		sharedLayout = graphicsWorld.Factory.CreateResourceLayout(resourceLayoutDescription);

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

		VertexLayoutDescription colorVertexLayout = new VertexLayoutDescription(
			new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
		colorVertexLayout.InstanceStepRate = 1;

		GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
			depthTestEnabled: false,
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
			ResourceLayouts = [sharedLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [RenderingResources.VertexLayout, colorVertexLayout, transformationVertexShaderParameterLayout],
				shaders: Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "DebugRenderable", graphicsWorld.Factory)
			),
			Outputs = graphicsWorld.RenderingOutputDescription
		};

		regularPipeline = graphicsWorld.Factory.CreateGraphicsPipeline(pipelineDescription);

		GraphicsPipelineDescription wireframePipelineDescription = new GraphicsPipelineDescription()
		{
			BlendState = BlendStateDescription.SingleOverrideBlend,
			DepthStencilState = new DepthStencilStateDescription(
			depthTestEnabled: false,
			depthWriteEnabled: true,
			comparisonKind: ComparisonKind.LessEqual),
			RasterizerState = new RasterizerStateDescription(
			cullMode: FaceCullMode.None,
			fillMode: PolygonFillMode.Wireframe,
			frontFace: FrontFace.Clockwise,
			depthClipEnabled: true,
			scissorTestEnabled: false
			),
			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = [sharedLayout],
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: [RenderingResources.VertexLayout, colorVertexLayout, transformationVertexShaderParameterLayout],
				shaders: Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "DebugRenderable", graphicsWorld.Factory)
			),
			Outputs = graphicsWorld.RenderingOutputDescription
		};

		wireframePipeline = graphicsWorld.Factory.CreateGraphicsPipeline(wireframePipelineDescription);

		BindableResource[] bindableResources = [GraphicsWorld.CameraProjViewBuffer];
		ResourceSetDescription resourceSetDescription = new(sharedLayout, bindableResources);
		resourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);
	}

	public void AddDrawCommands(CommandList cl, FixedDecimalLong8 _)
	{
		lock (sync)
		{
			if (!ShouldRender)
			{
				persistentRenderables.Clear();

				return;
			}

			Pipeline pipeline;
			if (Wireframe)
			{
				pipeline = wireframePipeline;
			}
			else
			{
				pipeline = regularPipeline;
			}

			AddDebugObjects?.Invoke();

			cl.PushDebugGroup("Debug objects");

			cl.SetPipeline(pipeline);
			cl.SetGraphicsResourceSet(0, resourceSet);

			foreach (var renderable in debugRenderables)
			{
				renderable.AddDrawCommands(cl, 0);
				//renderable.Dispose();
			}

			foreach (var renderable in persistentRenderables)
			{
				renderable.AddDrawCommands(cl, 0);
			}

			cl.PopDebugGroup();

			debugRenderables.Clear();
		}
	}

	private DebugRender(GraphicsWorld graphicsWorld)
	{
		GraphicsWorld = graphicsWorld;
	}

	public GraphicsWorld GraphicsWorld;

	private void GetBuffers(RgbaFloat color, Transform transform, out DeviceBuffer transformationBuffer, out DeviceBuffer colorBuffer)
	{
		lock (sync)
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
	}

	private Mesh GetCubeMesh(Vector3FixedDecimalInt4 size)
	{
		lock (sync)
		{
			if (!cubeMeshes.TryGetValue(size, out Mesh? mesh))
			{
				mesh = Utils.CreateDeviceResources(Utils.GetCubeVertexPositionTexture(size.ToVector3()), Utils.GetCubeIndices(), GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory);

				cubeMeshes.Add(size, mesh);
			}

			return mesh;
		}
	}

	public void DrawCube(Transform transform, RgbaFloat color) => DrawCube(transform, color, Vector3FixedDecimalInt4.One);

	public void DrawCube(Transform transform, RgbaFloat color, Vector3FixedDecimalInt4 scale)
	{
		lock (sync)
		{
			GetBuffers(color, new(transform), out DeviceBuffer transformationBuffer, out DeviceBuffer colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(scale), transformationBuffer, colorBuffer);

			debugRenderables.Add(renderable);
		}
	}

	public void PersistentCube(Transform transform, RgbaFloat color) => PersistentCube(transform, color, Vector3FixedDecimalInt4.One);

	public void PersistentCube(Transform transform, RgbaFloat color, Vector3FixedDecimalInt4 scale)
	{
		lock (sync)
		{
			GetBuffers(color, new(transform), out DeviceBuffer transformationBuffer, out DeviceBuffer colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(scale), transformationBuffer, colorBuffer);

			persistentRenderables.Add(renderable);
		}
	}

	public void DrawRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 ray, RgbaFloat color) => DrawRay(origin, ray, ray.Length(), color);

	public void DrawRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 length, RgbaFloat color)
	{
		lock (sync)
		{
			if (direction.LengthSquared() == 0) // Invert color to indicate that the vector has an invalid direction.
			{
				color = new(1 - color.R, 1 - color.G, 1 - color.B, 1 - color.A);
			}

			Transform transform = new(origin + (direction * length / 2), QuaternionFixedDecimalInt4.CreateLookingAt(direction, Vector3FixedDecimalInt4.UnitZ, Vector3FixedDecimalInt4.UnitY));

			GetBuffers(color, transform, out DeviceBuffer transformationBuffer, out DeviceBuffer colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(new((FixedDecimalInt4).1, (FixedDecimalInt4).1, length)), transformationBuffer, colorBuffer);

			debugRenderables.Add(renderable);
		}
	}

	public void PersistentRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 ray, RgbaFloat color) => PersistentRay(origin, ray, ray.Length(), color);

	public void PersistentRay(Vector3FixedDecimalInt4 origin, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 length, RgbaFloat color)
	{
		lock (sync)
		{
			if (direction.LengthSquared() == 0) // Invert color to indicate that the vector has an invalid direction.
			{
				color = new(1 - color.R, 1 - color.G, 1 - color.B, 1 - color.A);
			}

			Transform transform = new(origin, QuaternionFixedDecimalInt4.CreateLookingAt(direction, Vector3FixedDecimalInt4.UnitZ, Vector3FixedDecimalInt4.UnitY));


			GetBuffers(color, transform, out DeviceBuffer transformationBuffer, out DeviceBuffer colorBuffer);

			DebugRenderable renderable = new(GetCubeMesh(new((FixedDecimalInt4).1, (FixedDecimalInt4).1, length)), transformationBuffer, colorBuffer);

			persistentRenderables.Add(renderable);
		}
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

	public void DrawMesh(Mesh mesh, Transform transform, RgbaFloat color)
	{
		lock (sync)
		{
			GetBuffers(color, new(transform), out var transformationBuffer, out var colorBuffer);

			DebugRenderable renderable = new(mesh, transformationBuffer, colorBuffer);

			debugRenderables.Add(renderable);
		}
	}

	public void Reset()
	{
		lock (sync)
		{
			debugRenderables.Clear();
			persistentRenderables.Clear();
			AddDebugObjects = null;
		}
	}
}
