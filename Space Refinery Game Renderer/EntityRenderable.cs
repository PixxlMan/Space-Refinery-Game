using FixedPrecision;
using FXRenderer;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public class EntityRenderable : IRenderable
{
	private bool transformChangedSinceDraw;

	private Mesh mesh;

	private ResourceSet textureSet;

	private ResourceSet resourceSet;

	private DeviceBuffer transformationBuffer;

	private Pipeline pipeline;

	private GraphicsWorld graphicsWorld;

	private object SyncRoot;

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

	public static EntityRenderable Create(GraphicsWorld graphicsWorld, Transform transform, Mesh mesh, Texture texture, BindableResource cameraProjViewBuffer, BindableResource lightInfoBuffer)
	{
		EntityRenderable entityRenderable = new(transform);

		entityRenderable.mesh = mesh;

		TextureView textureView = graphicsWorld.Factory.CreateTextureView(texture);

		ResourceLayoutElementDescription[] textureLayoutDescriptions =
		{
				new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment)
			};
		ResourceLayout textureLayout = graphicsWorld.Factory.CreateResourceLayout(new ResourceLayoutDescription(textureLayoutDescriptions));

		entityRenderable.textureSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(textureLayout, textureView, graphicsWorld.GraphicsDevice.Aniso4xSampler));

		ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
			{
				new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
			};
		ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
		ResourceLayout sharedLayout = graphicsWorld.Factory.CreateResourceLayout(resourceLayoutDescription);

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
		entityRenderable.transformationBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));

		graphicsWorld.GraphicsDevice.UpdateBuffer(entityRenderable.transformationBuffer, 0, entityRenderable.Transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
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
			ResourceLayouts = new ResourceLayout[] { sharedLayout, textureLayout },
			ShaderSet = new ShaderSetDescription(
				vertexLayouts: new VertexLayoutDescription[] { vertexLayout, transformationVertexShaderParameterLayout },
				shaders: graphicsWorld.ShaderLoader.LoadCached("EntityRenderable")
			),
			Outputs = graphicsWorld.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription
		};

		entityRenderable.pipeline = graphicsWorld.Factory.CreateGraphicsPipeline(pipelineDescription);

		BindableResource[] bindableResources = new BindableResource[] { lightInfoBuffer, cameraProjViewBuffer };
		ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
		entityRenderable.resourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);

		entityRenderable.graphicsWorld = graphicsWorld;

		graphicsWorld.AddRenderable(entityRenderable);

		return entityRenderable;
	}

	public void AddDrawCommands(CommandList commandList)
	{
		if (transformChangedSinceDraw)
			commandList.UpdateBuffer(transformationBuffer, 0, Transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		commandList.SetPipeline(pipeline);
		commandList.SetGraphicsResourceSet(0, resourceSet);
		commandList.SetGraphicsResourceSet(1, textureSet);
		commandList.SetVertexBuffer(0, mesh.VertexBuffer);
		commandList.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
		commandList.SetVertexBuffer(1, transformationBuffer);

		commandList.DrawIndexed(mesh.IndexCount);
	}

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
