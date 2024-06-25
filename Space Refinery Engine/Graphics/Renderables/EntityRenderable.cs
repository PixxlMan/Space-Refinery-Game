using FixedPrecision;
using System.Numerics;
using Veldrid;
using static Space_Refinery_Engine.Renderer.RenderingResources;

namespace Space_Refinery_Engine.Renderer;

public sealed class EntityRenderable : IRenderable
{
	private Mesh mesh;

	private Material material;

	private ResourceSet resourceSet;

	private DeviceBuffer transformationBuffer;

	private GraphicsWorld graphicsWorld;

	private Pipeline pipeline;

	private readonly object SyncRoot = new();

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
	}

	public Transform Transform;

	public static EntityRenderable CreateAndAdd(GraphicsWorld graphicsWorld, Transform transform, Mesh mesh, Material material, BindableResource cameraProjViewBuffer, BindableResource lightInfoBuffer)
	{
		EntityRenderable entityRenderable = new(transform)
		{
			mesh = mesh,
			material = material,
			transformationBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes, BufferUsage.VertexBuffer))
		};

		graphicsWorld.GraphicsDevice.UpdateBuffer(entityRenderable.transformationBuffer, 0, entityRenderable.Transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		BindableResource[] bindableResources = [lightInfoBuffer, cameraProjViewBuffer];
		ResourceSetDescription resourceSetDescription = new(SharedLayout, bindableResources);
		entityRenderable.resourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);

		entityRenderable.graphicsWorld = graphicsWorld;

		graphicsWorld.AddRenderable(entityRenderable);

		switch (mesh.WindingOrder)
		{
			case FrontFace.Clockwise:
				entityRenderable.pipeline = ClockwisePipelineResource;
				break;
			case FrontFace.CounterClockwise:
				entityRenderable.pipeline = CounterClockwisePipelineResource;
				break;
		}

		return entityRenderable;
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		commandList.UpdateBuffer(transformationBuffer, 0, Transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero));

		commandList.SetPipeline(pipeline);
		commandList.SetGraphicsResourceSet(0, resourceSet);
		material.AddSetCommands(commandList);
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

public struct AuxillaryData
{
	public AuxillaryData(Vector3 cameraPosition)
	{
		CameraPosition = cameraPosition;
	}

	public const int SizeInBytes = 16; // Size has to be a multiple of 16! //4;
	public Vector3 CameraPosition;
}
