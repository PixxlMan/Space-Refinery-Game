using FixedPrecision;
using FXRenderer;
using SharpDX;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public sealed class EntityRenderable : IRenderable
{
	private bool transformChangedSinceDraw;

	private Mesh mesh;

	private ResourceSet textureSet;

	private ResourceSet resourceSet;

	private ResourceSet pbrSet;

	private DeviceBuffer transformationBuffer;

	private GraphicsWorld graphicsWorld;

	private object SyncRoot = new();

	private bool shouldDraw = true;
	public bool ShouldDraw
	{
		get => shouldDraw;
		set
		{
			lock (SyncRoot)
			{
				if (!value)
				{
					graphicsWorld.RemoveRenderable(this);
				}
				else
				{
					graphicsWorld.AddRenderable(this);
				}

				shouldDraw = value;
			}
		}
	}

	private EntityRenderable(Transform transform)
	{
		Transform = transform;

		Transform.TransformChanged += (_) => transformChangedSinceDraw = true;
	}

	public Transform Transform;

	public static bool HasCreatedStaticDeviceResources { get; private set; } = false;

	public static ResourceLayout PBRDataLayout { get; private set; }

	public static ResourceLayout AuxillaryDataLayout { get; private set; }

	public static ResourceLayout TextureLayout { get; private set; }

	public static ResourceLayout SharedLayout { get; private set; }

	public static VertexLayoutDescription TransformationVertexShaderParameterLayout { get; private set; }

	public static VertexLayoutDescription VertexLayout { get; private set; }

	public static Pipeline Pipeline { get; private set; }

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
			new VertexElementDescription("InstanceRotationM33", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
			new VertexElementDescription("InstanceScale", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
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

		Pipeline = graphicsWorld.Factory.CreateGraphicsPipeline(pipelineDescription);

		HasCreatedStaticDeviceResources = true;
	}

	public static EntityRenderable Create(GraphicsWorld graphicsWorld, Transform transform, Mesh mesh, Texture texture, BindableResource cameraProjViewBuffer, BindableResource lightInfoBuffer)
	{
		if (!HasCreatedStaticDeviceResources)
		{
			CreateStaticDeviceResources(graphicsWorld);
		}

		EntityRenderable entityRenderable = new(transform);

		entityRenderable.mesh = mesh;

		TextureView textureView = graphicsWorld.Factory.CreateTextureView(texture);

		DeviceBuffer pbrBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(PBRData.SizeInBytes, BufferUsage.UniformBuffer));
		graphicsWorld.GraphicsDevice.UpdateBuffer(pbrBuffer, 0, new PBRData(0.75f, 0.25f, .5f));
		entityRenderable.pbrSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(PBRDataLayout, pbrBuffer));

		entityRenderable.textureSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(TextureLayout, textureView, graphicsWorld.GraphicsDevice.Aniso4xSampler));

		entityRenderable.transformationBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));

		graphicsWorld.GraphicsDevice.UpdateBuffer(entityRenderable.transformationBuffer, 0, entityRenderable.Transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		BindableResource[] bindableResources = new BindableResource[] { lightInfoBuffer, cameraProjViewBuffer };
		ResourceSetDescription resourceSetDescription = new ResourceSetDescription(SharedLayout, bindableResources);
		entityRenderable.resourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);

		entityRenderable.graphicsWorld = graphicsWorld;

		graphicsWorld.AddRenderable(entityRenderable);

		return entityRenderable;
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		if (transformChangedSinceDraw)
			commandList.UpdateBuffer(transformationBuffer, 0, Transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		commandList.SetPipeline(Pipeline);
		commandList.SetGraphicsResourceSet(0, resourceSet);
		commandList.SetGraphicsResourceSet(1, textureSet);
		commandList.SetVertexBuffer(0, mesh.VertexBuffer);
		commandList.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
		commandList.SetVertexBuffer(1, transformationBuffer);

		commandList.DrawIndexed(mesh.IndexCount);
	}

	public void Destroy()
	{
		lock (SyncRoot)
		{
			if (shouldDraw)
			{
				graphicsWorld.RemoveRenderable(this);
			}

			shouldDraw = false;
		}			
	}
}

public struct PBRData
{
	public PBRData(float metallic, float roughness, float ao)
	{
		Metallic = metallic;
		Roughness = roughness;
		Ao = ao;
	}

	public const int SizeInBytes = 16; // Size has to be a multiple of 16! //4;
	public float Metallic;
	public float Roughness;
	public float Ao;
}

public struct AuxillaryData
{
	public AuxillaryData(Vector3 cameraPosition)
	{
		CameraPosition = cameraPosition;
	}

	public const int SizeInBytes = 16; // Size has to be a multiple of 16! //4;
	public Vector3 CameraPosition;
}
