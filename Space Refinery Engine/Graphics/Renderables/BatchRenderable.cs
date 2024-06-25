using FixedPrecision;
using Space_Refinery_Engine;
using Veldrid;
using static Space_Refinery_Engine.Renderer.RenderingResources;

namespace Space_Refinery_Engine.Renderer;

public sealed partial class BatchRenderable : IRenderable
{
	private Mesh mesh;

	private Material material;

	private ResourceSet resourceSet;

	private DeviceBuffer transformationsBuffer;

	private GraphicsWorld graphicsWorld;

	private Pipeline pipeline;

	private readonly object syncRoot = new();

	public string Name { get; private set; }

	private bool shouldDraw = true;
	public bool ShouldDraw
	{
		get => shouldDraw;
		set
		{
			lock (syncRoot)
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


	private List<BlittableTransform> transforms = new((int)initialCapacity);
	// Is it even neccessary to keep data? Everything is already on the GPU, and if I won't do any compacting anyways... anycase, just keep bools to keep track occupation, which also allows for compacting, although talking that much with the GPU to compact with it might not be ideal...?

	private PriorityQueue<int, int> availableIndexesQueue = new();

	private Dictionary<object, int> transformsDictionary = new((int)initialCapacity, ReferenceEqualityComparer.Instance);

	private static readonly Transform noEntryTransform = new Transform(new(FixedDecimalInt4.MaxValue, FixedDecimalInt4.MaxValue, FixedDecimalInt4.MaxValue), QuaternionFixedDecimalInt4.Zero)/*.GetBlittableTransform(Vector3FixedDecimalInt4.Zero)*/;


	private uint currentCapacity;

	private const uint initialCapacity = 128;


	public void CreateBatchRenderableEntity(Transform transform, object associatedObject)
	{
		lock (syncRoot)
		{
			var blittableTransform = transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero);

			int index = AppendTransformsList(transform);

			transformsDictionary.Add(associatedObject, index);

			ManageTransformsBuffer();

			UpdateTransform(associatedObject, transform);

			switch (mesh.WindingOrder)
			{
				case FrontFace.Clockwise:
					pipeline = ClockwisePipelineResource;
					break;
				case FrontFace.CounterClockwise:
					pipeline = CounterClockwisePipelineResource;
					break;
			}
		}
	}

	private int AppendTransformsList(Transform transform)
	{
		var blittableTransform = transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero);

		if (availableIndexesQueue.Count != 0)
		{
			// If there are availables indexes in the list from previous deletions
			int index = availableIndexesQueue.Dequeue();
			transforms[index] = blittableTransform;
			return index;
		}
		else
		{
			// If there are not any available indexes we must create new ones
			transforms.Add(blittableTransform);
			return transforms.Count - 1;
		}
	}

	public void RemoveBatchRenderableEntity(object associatedObject)
	{
		lock (syncRoot)
		{
			int index = transformsDictionary[associatedObject];

			UpdateTransform(associatedObject, noEntryTransform);

			availableIndexesQueue.Enqueue(index, index);

			transformsDictionary.Remove(associatedObject);

			ManageTransformsBuffer();
		}
	}

	public void Reset()
	{
		lock (syncRoot)
		{
			shouldDraw = true;

			Clear();
		}
	}

	public void Restore()
	{
		graphicsWorld.AddRenderable(this);
	}

	public void Clear()
	{
		lock (syncRoot)
		{
			Logging.LogDebug($"Clearing {nameof(BatchRenderable)} '{Name}'.");

			transformsDictionary.Clear();

			availableIndexesQueue.Clear();

			transforms.Clear();

			ManageTransformsBuffer();
		}
	}

	public void UpdateTransform(object associatedObject, Transform transform)
	{
		lock (syncRoot)
		{
			var blittableTransform = transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero);

			int index = transformsDictionary[associatedObject];

			transforms[index] = blittableTransform;

			// TODO: perform bulk updates every frame when necessary instead of doing it on demand every time?

			graphicsWorld.GraphicsDevice.UpdateBuffer(transformationsBuffer, (uint)index * BlittableTransform.SizeInBytes, ref blittableTransform, BlittableTransform.SizeInBytes);			
		}
	}

	public static BatchRenderable CreateAndAdd(string name, GraphicsWorld graphicsWorld, Mesh mesh, Material material, BindableResource cameraProjViewBuffer, BindableResource lightInfoBuffer)
	{
		BatchRenderable batchRenderable = new()
		{
			Name = name,
			mesh = mesh,
			material = material,
		};

		BindableResource[] bindableResources = [lightInfoBuffer, cameraProjViewBuffer];
		ResourceSetDescription resourceSetDescription = new ResourceSetDescription(SharedLayout, bindableResources);
		batchRenderable.resourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);

		batchRenderable.graphicsWorld = graphicsWorld;

		batchRenderable.ManageTransformsBuffer();

		RegisterBatchRenderable(batchRenderable);

		batchRenderable.Restore();

		return batchRenderable;
	}

	private void ManageTransformsBuffer()
	{
		lock (syncRoot)
		{
			// The buffer has not been created yet and should be initialized to the initialCapacity.
			if (currentCapacity == 0 || transformationsBuffer is null)
			{
				transformationsBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes * initialCapacity, BufferUsage.VertexBuffer));
				transformationsBuffer.Name = $"{Name} Transformation Buffer";

				currentCapacity = initialCapacity;

				ReuploadTransformsBuffer();

				return;
			}

			// OPTIMIZE: It might be possible to improve performance in the rest of the method by copying data from old transformation buffer instead of reuploading it. It would need access to a CommandList though.

			// Too large capacity, should recreate buffer, smaller.
			// Precisely, if the capacity is four times greater than the number of actual transforms, and 32 additional transforms (to ensure there is room for the number of transforms to grow without needing to resize the buffer), then resize the buffer.
			// The reason this check requires such a large difference between capacity and transform count is that resizing and/or reuploading the buffer is a relatively expensive process, preferably avoided.
			if ((currentCapacity / 4) - 32 > TransformsCount)
			{
				Logging.LogDebug($"Shrinking the buffer of {nameof(BatchRenderable)} '{Name}'.");

				var oldTransformationsBuffer = transformationsBuffer;

				uint newCapacity = 0;

				if (TransformsCount == 0)
				{
					newCapacity = initialCapacity;
				}
				else
				{
					newCapacity = currentCapacity / 2;
				}

				transformationsBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes * newCapacity, BufferUsage.VertexBuffer));
				transformationsBuffer.Name = $"{Name} Transformation Buffer";

				currentCapacity = newCapacity;

				oldTransformationsBuffer.Dispose(); // TODO? Hmm why do this afterwards? Should check this out...

				ReuploadTransformsBuffer();

				return;
			}

			// Number of transforms exceeds capacity, needs to recreate buffer, but bigger.
			else if (TransformsCount > currentCapacity)
			{
				Logging.LogDebug($"Growing the buffer of {nameof(BatchRenderable)} '{Name}'.");

				var oldTransformationsBuffer = transformationsBuffer;

				uint newCapacity = currentCapacity * 2;

				transformationsBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes * newCapacity, BufferUsage.VertexBuffer));

				currentCapacity = newCapacity;

				oldTransformationsBuffer.Dispose();

				ReuploadTransformsBuffer();

				return;
			}
		}
	}

	private int TransformsCount
	{
		get => transformsDictionary.Count;
	}

	private void ReuploadTransformsBuffer()
	{
		lock (syncRoot)
		{
			graphicsWorld.GraphicsDevice.UpdateBuffer(transformationsBuffer, 0, transforms.ToArray());
		}
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		lock (syncRoot)
		{
			if (TransformsCount == 0 || !shouldDraw)
			{
				return;
			}

			commandList.SetPipeline(pipeline);
			commandList.SetGraphicsResourceSet(0, resourceSet);
			material.AddSetCommands(commandList);
			commandList.SetVertexBuffer(0, mesh.VertexBuffer);
			commandList.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
			commandList.SetVertexBuffer(1, transformationsBuffer);

			commandList.DrawIndexed(mesh.IndexCount, (uint)transforms.Count, 0, 0, 0);
		}
	}

	public void Destroy()
	{
		lock (syncRoot)
		{
			if (shouldDraw)
			{
				graphicsWorld.RemoveRenderable(this);
			}

			shouldDraw = false;

			UnregisterBatchRenderable(this);
		}			
	}
}