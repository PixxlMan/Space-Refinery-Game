using FixedPrecision;
using FXRenderer;
using SharpDX;
using Space_Refinery_Utilities;
using System.Numerics;
using System.Transactions;
using Veldrid;
using static Space_Refinery_Game_Renderer.RenderingResources;

namespace Space_Refinery_Game_Renderer;

public sealed class BatchRenderable : IRenderable
{
	private Mesh mesh;

	private ResourceSet textureSet;

	private ResourceSet resourceSet;

	private ResourceSet pbrSet;

	private DeviceBuffer transformationsBuffer;

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

	private OrderedDictionary<BatchRenderableEntityHandle, BlittableTransform> transformsDictionary = new();

	private uint currentCapacity;

	private const uint initialCapacity = 128;

	public BatchRenderableEntityHandle CreateBatchRenderableEntity(Transform transform) // Seems there is a bug where a renderable entity is assigned to the wrong entity?
	{
		lock (SyncRoot)
		{
			BatchRenderableEntityHandle handle = new(0);

			while (transformsDictionary.ContainsKey(handle)) // Generate unique handle. Should be sufficently efficent.
			{
				handle.Handle = Random.Shared.Next(int.MinValue, int.MaxValue);
			}

			var blittableTransform = transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero);

			transformsDictionary.Add(handle, blittableTransform);

			ManageTransformsBuffer();

			UpdateTransform(handle, transform);

			return handle;
		}
	}

	public void RemoveBatchRenderableEntity(BatchRenderableEntityHandle handle)
	{
		lock (SyncRoot)
		{
			transformsDictionary.Remove(handle);

			ManageTransformsBuffer();
		}
	}

	public void UpdateTransform(BatchRenderableEntityHandle handle, Transform transform)
	{
		lock (SyncRoot)
		{
			transformsDictionary[handle] = transform.GetBlittableTransform(Vector3FixedDecimalInt4.Zero);

			graphicsWorld.GraphicsDevice.UpdateBuffer(transformationsBuffer, (uint)transformsDictionary.IndexOf(handle) * BlittableTransform.SizeInBytes, transformsDictionary[handle]);
		}
	}

	public static BatchRenderable Create(GraphicsWorld graphicsWorld, PBRData pBRData, Mesh mesh, Texture texture, BindableResource cameraProjViewBuffer, BindableResource lightInfoBuffer)
	{
		BatchRenderable batchRenderable = new()
		{
			mesh = mesh
		};

		TextureView textureView = graphicsWorld.Factory.CreateTextureView(texture);

		DeviceBuffer pbrBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(PBRData.SizeInBytes, BufferUsage.UniformBuffer));
		graphicsWorld.GraphicsDevice.UpdateBuffer(pbrBuffer, 0, pBRData);
		batchRenderable.pbrSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(PBRDataLayout, pbrBuffer));

		batchRenderable.textureSet = graphicsWorld.Factory.CreateResourceSet(new ResourceSetDescription(TextureLayout, textureView, graphicsWorld.GraphicsDevice.Aniso4xSampler));

		BindableResource[] bindableResources = new BindableResource[] { lightInfoBuffer, cameraProjViewBuffer };
		ResourceSetDescription resourceSetDescription = new ResourceSetDescription(SharedLayout, bindableResources);
		batchRenderable.resourceSet = graphicsWorld.Factory.CreateResourceSet(resourceSetDescription);

		batchRenderable.graphicsWorld = graphicsWorld;

		batchRenderable.ManageTransformsBuffer();

		graphicsWorld.AddRenderable(batchRenderable);

		return batchRenderable;
	}

	private void ManageTransformsBuffer()
	{
		lock (SyncRoot)
		{
			// The buffer has not been created yet and should be initialized to the initialCapacity.
			if (currentCapacity == 0 || transformationsBuffer is null)
			{
				transformationsBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes * initialCapacity, BufferUsage.VertexBuffer));

				currentCapacity = initialCapacity;

				ReuploadTransformsBuffer();

				return;
			}

			// OPTIMIZE: It might be possible to improve performance in the rest of the method by copying data from old transformation buffer instead of reuploading it. It would need access to a CommandList though.

			// Too large capacity, should recreate buffer, smaller.
			if ((currentCapacity / 4) - 32 > transformsDictionary.Count)
			{
				var oldTransformationsBuffer = transformationsBuffer;

				transformationsBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes * currentCapacity / 2, BufferUsage.VertexBuffer));

				currentCapacity = currentCapacity / 2;

				oldTransformationsBuffer.Dispose();

				ReuploadTransformsBuffer();

				return;
			}

			// Number of transforms exceeds capacity, needs to recreate buffer, but bigger.
			if (transformsDictionary.Count > currentCapacity)
			{
				var oldTransformationsBuffer = transformationsBuffer;

				transformationsBuffer = graphicsWorld.Factory.CreateBuffer(new BufferDescription(BlittableTransform.SizeInBytes * currentCapacity * 2, BufferUsage.VertexBuffer));

				currentCapacity = currentCapacity * 2;

				oldTransformationsBuffer.Dispose();

				ReuploadTransformsBuffer();

				return;
			}
		}
	}

	private void ReuploadTransformsBuffer()
	{
		lock (SyncRoot)
		{
			graphicsWorld.GraphicsDevice.UpdateBuffer(transformationsBuffer, 0, transformsDictionary.Values.ToArray());
		}
	}

	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime)
	{
		lock (SyncRoot)
		{
			if (transformsDictionary.Count == 0)
			{
				return;
			}

			commandList.SetPipeline(PipelineResource);
			commandList.SetGraphicsResourceSet(0, resourceSet);
			commandList.SetGraphicsResourceSet(1, textureSet);
			commandList.SetVertexBuffer(0, mesh.VertexBuffer);
			commandList.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
			commandList.SetVertexBuffer(1, transformationsBuffer);

			commandList.DrawIndexed(mesh.IndexCount, (uint)transformsDictionary.Count, 0, 0, 0);
		}
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

public struct BatchRenderableEntityHandle : IEquatable<BatchRenderableEntityHandle>
{
	public BatchRenderableEntityHandle(int handle)
	{
		this.Handle = handle;
	}

	internal int Handle;

	public override bool Equals(object obj)
	{
		return obj is BatchRenderableEntityHandle handle && Equals(handle);
	}

	public bool Equals(BatchRenderableEntityHandle other)
	{
		return Handle == other.Handle;
	}

	public override int GetHashCode()
	{
		return Handle;
	}

	public static bool operator ==(BatchRenderableEntityHandle left, BatchRenderableEntityHandle right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BatchRenderableEntityHandle left, BatchRenderableEntityHandle right)
	{
		return !(left == right);
	}
}