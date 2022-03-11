using FixedPrecision;
using FXRenderer;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public class EntityRenderable : IRenderable, ITransformable
{
	private bool transformChangedSinceDraw;

	private Mesh mesh;

	private ResourceSet textureSet;

	private ResourceSet resourceSet;

	private DeviceBuffer transformationBuffer;

	private Pipeline pipeline;

	private Vector3FixedDecimalInt4 position;
	private QuaternionFixedDecimalInt4 rotation;
	private Vector3FixedDecimalInt4 scale;

	private EntityRenderable()
	{
		TransformChanged += (_) => transformChangedSinceDraw = true;
	}

	public Vector3FixedDecimalInt4 Position { get => position; set { position = value; TransformChanged?.Invoke(this); } }
	public QuaternionFixedDecimalInt4 Rotation { get => rotation; set { rotation = value; TransformChanged?.Invoke(this); } }
	public Vector3FixedDecimalInt4 Scale { get => scale; set { scale = value; TransformChanged?.Invoke(this); } }

	public event Action<ITransformable> TransformChanged;

	public static EntityRenderable Create(GraphicsDevice gd, ResourceFactory factory, ITransformable transform, Mesh mesh, Texture texture, BindableResource cameraProjViewBuffer, BindableResource lightInfoBuffer)
	{
		EntityRenderable entityRenderable = new();

		((ITransformable)entityRenderable).CopyTransform(transform);

		entityRenderable.mesh = mesh;

		TextureView textureView = factory.CreateTextureView(texture);

		ResourceLayoutElementDescription[] textureLayoutDescriptions =
		{
				new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment)
			};
		ResourceLayout textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(textureLayoutDescriptions));

		entityRenderable.textureSet = factory.CreateResourceSet(new ResourceSetDescription(textureLayout, textureView, gd.Aniso4xSampler));

		ResourceLayoutElementDescription[] resourceLayoutElementDescriptions =
			{
				new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
				new ResourceLayoutElementDescription("ProjView", ResourceKind.UniformBuffer, ShaderStages.Vertex),
			};
		ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
		ResourceLayout sharedLayout = factory.CreateResourceLayout(resourceLayoutDescription);


		VertexLayoutDescription transformationVertexShaderParameterLayout = new VertexLayoutDescription(
			new VertexElementDescription("InstancePosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
			new VertexElementDescription("InstanceRotation", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
			new VertexElementDescription("InstanceScale", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
			);
		transformationVertexShaderParameterLayout.InstanceStepRate = 1;
		entityRenderable.transformationBuffer = factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer));

		gd.UpdateBuffer(entityRenderable.transformationBuffer, 0, ((ITransformable)entityRenderable).GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

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
				shaders: Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), "EntityRenderable", factory)
			),
			Outputs = gd.MainSwapchain.Framebuffer.OutputDescription
		};

		entityRenderable.pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

		BindableResource[] bindableResources = new BindableResource[] { lightInfoBuffer, cameraProjViewBuffer };
		ResourceSetDescription resourceSetDescription = new ResourceSetDescription(sharedLayout, bindableResources);
		entityRenderable.resourceSet = factory.CreateResourceSet(resourceSetDescription);

		return entityRenderable;
	}

	public void AddDrawCommands(CommandList commandList)
	{
		if (transformChangedSinceDraw)
			commandList.UpdateBuffer(transformationBuffer, 0, ((ITransformable)this).GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		commandList.SetPipeline(pipeline);
		commandList.SetGraphicsResourceSet(0, resourceSet);
		commandList.SetGraphicsResourceSet(1, textureSet);
		commandList.SetVertexBuffer(0, mesh.VertexBuffer);
		commandList.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
		commandList.SetVertexBuffer(1, transformationBuffer);

		commandList.DrawIndexed(mesh.IndexCount);
	}
}
