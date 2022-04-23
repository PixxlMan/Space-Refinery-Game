using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Game_Renderer;

public class GraphicsWorld
{
	public List<IRenderable> UnorderedRenderables = new();

	public SortedDictionary<int, List<IRenderable>> SpecificOrderRenderables = new(); // Use Lookup<int, IRenderable> and sort manually as it's not very common to add objects?

	public GraphicsDevice GraphicsDevice;

	public ResourceFactory Factory;

	public Swapchain Swapchain;

	private CommandList commandList;

	public DeviceBuffer CameraProjViewBuffer;

	public DeviceBuffer LightInfoBuffer;

	private Vector3FixedDecimalInt4 lightDir;

	public DeviceBuffer ViewInfoBuffer;

	private Window window;

	public string SynchronizationObject = "69";

	public Camera Camera;

	public event Action<CommandList> CustomDrawOperations;

	public void SetUp(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.window = window;

		window.Resized += () => Camera.WindowResized(window.Width, window.Height);

		Camera = new(window.Width, window.Height);

		Camera.Position = new Vector3FixedDecimalInt4(0, 0, 10);
		Camera.Pitch = 0;
		Camera.Yaw = 0;

		Camera.FarDistance = 10000;

		Camera.NearDistance = "0.1".Parse<FixedDecimalInt4>();

		CreateDeviceObjects(gd, factory, swapchain);
	}

	public void Run()
	{
		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();

			FixedDecimalInt4 timeLastUpdate = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalInt4>();
			FixedDecimalInt4 time;
			FixedDecimalInt4 deltaTime;
			while (window.Exists)
			{
				time = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalInt4>();

				deltaTime = time - timeLastUpdate;

				timeLastUpdate = time;

				Thread.Sleep(1);

				RenderScene(deltaTime);
			}
		}));

		thread.Start();
	}

	private void CreateDeviceObjects(GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.GraphicsDevice = gd;
		this.Factory = factory;
		this.Swapchain = swapchain;

		commandList = factory.CreateCommandList();

		CameraProjViewBuffer = factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		LightInfoBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		lightDir = Vector3FixedDecimalInt4.Normalize(new Vector3FixedDecimalInt4("0.3".Parse<FixedDecimalInt4>(), "-0.75".Parse<FixedDecimalInt4>(), "-0.3".Parse<FixedDecimalInt4>()));

		ViewInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MatrixPair>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
	}

	public void AddRenderable(IRenderable renderable)
	{
		UnorderedRenderables.Add(renderable);
	}

	public void AddRenderable(IRenderable renderable, int order)
	{
		if (SpecificOrderRenderables.ContainsKey(order))
		{
			SpecificOrderRenderables[order].Add(renderable);
		}
		else
		{
			SpecificOrderRenderables.Add(order, new() { renderable });
		}
	}

	private void RenderScene(FixedDecimalInt4 deltaTime)
	{
		lock(SynchronizationObject)
		{
			// Begin() must be called before commands can be issued.
			commandList.Begin();

			// Update per-frame resources.
			commandList.UpdateBuffer(CameraProjViewBuffer, 0, new MatrixPair(Camera.ViewMatrix.ToMatrix4x4(), Camera.ProjectionMatrix.ToMatrix4x4()));

			commandList.UpdateBuffer(LightInfoBuffer, 0, new LightInfo(lightDir.ToVector3(), Camera.Position.ToVector3()));

			Matrix4x4.Invert(Camera.ProjectionMatrix.ToMatrix4x4(), out Matrix4x4 inverseProjection);
			Matrix4x4.Invert(Camera.ViewMatrix.ToMatrix4x4(), out Matrix4x4 inverseView);
			commandList.UpdateBuffer(ViewInfoBuffer, 0, new MatrixPair(
				inverseProjection,
				inverseView));

			// We want to render directly to the output window.
			commandList.SetFramebuffer(Swapchain.Framebuffer);
			commandList.ClearColorTarget(0, RgbaFloat.White);
			commandList.ClearDepthStencil(1f);

			commandList.PushDebugGroup("Draw renderables");
			if (SpecificOrderRenderables.Count == 0)
			{
				foreach (var renderable in UnorderedRenderables)
				{
					renderable.AddDrawCommands(commandList);
				}
			}
			else
			{
				bool hasRenderedUnorderedRenderables = false;
				foreach (var index in SpecificOrderRenderables.Keys)
				{
					if (index >= 0 && !hasRenderedUnorderedRenderables)
					{
						foreach (var renderable in UnorderedRenderables)
						{
							renderable.AddDrawCommands(commandList);

							hasRenderedUnorderedRenderables = true;
						}
					}

					foreach (var renderable in SpecificOrderRenderables[index])
					{
						renderable.AddDrawCommands(commandList);
					}
				}

				if (!hasRenderedUnorderedRenderables)
				{
					foreach (var renderable in UnorderedRenderables)
					{
						renderable.AddDrawCommands(commandList);
					}
				}
			}
			commandList.PopDebugGroup();

			CustomDrawOperations?.Invoke(commandList);

			// End() must be called before commands can be submitted for execution.
			commandList.End();
			GraphicsDevice.SubmitCommands(commandList);
			GraphicsDevice.WaitForIdle();

			// Once commands have been submitted, the rendered image can be presented to the application window.
			GraphicsDevice.SwapBuffers(Swapchain);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LightInfo
	{
		public BlittableVector3 LightDirection;
		private float padding0;
		public BlittableVector3 CameraPosition;
		private float padding1;

		public LightInfo(Vector3 lightDirection, Vector3 cameraPosition)
		{
			LightDirection = new BlittableVector3(lightDirection);
			CameraPosition = new BlittableVector3(cameraPosition);
			padding0 = 0;
			padding1 = 0;
		}
	}
}
