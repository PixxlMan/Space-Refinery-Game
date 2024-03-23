using FixedPrecision;
using FXRenderer;
using SixLabors.ImageSharp;
using Space_Refinery_Utilities;
using Space_Refinery_Utilities.Units;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

/// <remarks>
/// <para>
/// Lifetime:
/// There is only one GraphicsWorld in the game and it's lifetime will last during the entire lifetime of the application.
/// The reason is that it is very expensive create and intialize this object.
/// </para>
/// <para>
/// Thread safety:
/// The GraphicsWorld is entierly thread safe, although not all exposed objects are necessarily thread safe! Caution needs to be exercised when using
/// the exposed graphics implementation details!
/// </para>
/// </remarks>
public sealed class GraphicsWorld
{
	private bool setUp = false;

	private HashSet<IRenderable> unorderedRenderables = new();

	private SortedDictionary<int, HashSet<IRenderable>> specificOrderRenderables = new(); // Use Lookup<int, IRenderable> and sort manually as it's not very common to add objects?

	private ConcurrentDictionary<IRenderable, int> renderableToOrder = new();

	private GraphicsDevice graphicsDevice;
	public GraphicsDevice GraphicsDevice
	{
		get
		{
			lock (graphicsDevice)
			{
				return graphicsDevice;
			}
		}
		set
		{
			lock (graphicsDevice)
			{
				graphicsDevice = value;
			}
		}
	}

	private ResourceFactory factory;
	public ResourceFactory Factory
	{
		get
		{
			lock (factory)
			{
				return factory;
			}
		}
		set
		{
			lock (factory)
			{
				factory = value;
			}
		}
	}

	private Swapchain swapchain;
	public Swapchain Swapchain
	{
		get
		{
			lock (swapchain)
			{
				return swapchain;
			}
		}

		set
		{
			lock (swapchain)
			{
				swapchain = value;
			}
		}
	}

	private CommandList commandList;

	private DeviceBuffer cameraProjViewBuffer;
	public DeviceBuffer CameraProjViewBuffer
	{
		get
		{
			lock (cameraProjViewBuffer)
			{
				return cameraProjViewBuffer;
			}
		}

		set
		{
			lock (cameraProjViewBuffer)
			{
				cameraProjViewBuffer = value;
			}
		}
	}

	private DeviceBuffer lightInfoBuffer;
	public DeviceBuffer LightInfoBuffer
	{
		get
		{
			lock (lightInfoBuffer)
			{
				return lightInfoBuffer;
			}
		}

		set
		{
			lock (lightInfoBuffer)
			{
				lightInfoBuffer = value;
			}
		}
	}

	private Vector3FixedDecimalInt4 lightDir;

	private DeviceBuffer viewInfoBuffer;
	public DeviceBuffer ViewInfoBuffer
	{
		get
		{
			lock (viewInfoBuffer)
			{
				return viewInfoBuffer;
			}
		}

		set
		{
			lock (viewInfoBuffer)
			{
				viewInfoBuffer = value;
			}
		}
	}

	private Window window;
	public Window Window
	{
		get
		{
			lock (window)
			{
				return window;
			}
		}
		private set
		{
			lock (window)
			{
				window = value;
			}
		}
	}

	public MeshLoader MeshLoader { get; private set; }

	public MaterialLoader MaterialLoader { get; private set; }

	public ShaderLoader ShaderLoader { get; private set; }

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock(responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.
	
	private Camera camera;
	public Camera Camera
	{
		get
		{
			lock (camera)
			{
				return camera;
			}
		}
		private set
		{
			lock (camera)
			{
				camera = value;
			}
		}
	}

	public IntervalUnit FrametimeLowerLimit = 0.001;

	public bool ShouldLimitFramerate = true;

	public WeakEvent<CommandList>? CustomDrawOperations;

	public WeakEvent? FrameRendered;

	public WeakEvent<IntervalUnit>? CollectRenderingPerformanceData;

	public WeakEvent<(int, int)>? WindowResized;

	public void SetUp(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		Debug.Assert(!setUp, "The GraphicsWorld has already been set up!");

		setUp = true;

		Logging.LogScopeStart("Setting up GraphicsWorld");

		// No dependency
		Configuration.Default.PreferContiguousImageBuffers = true; // Use contigous image buffers in ImageSharp to load textures.
																   // This is necessary for them to uploadable to the GPU!

		// No dependency
		ShaderLoader = new(this);


		// No dependency
		this.window = window;

		Window.Resized += HandleWindowResized;


		// Depends on Window
		camera = new(window.Width, window.Height, Perspective.Perspective);

		Camera.Transform.Position = new Vector3FixedDecimalInt4(0, 0, 10);

		Camera.FarDistance = 10000;

		Camera.NearDistance = (FixedDecimalInt4)0.1;

		Camera.FieldOfView = 75 * FixedDecimalInt4.DegreesToRadians;


		// These depend on the shader loader
		CreateDeviceObjects(gd, factory, swapchain);

		RenderingResources.CreateStaticDeviceResources(this);


		// These depend on the device resources.
		MeshLoader = new(this);
		
		// This depends on using contigous image buffers.
		MaterialLoader = new(this);

		Logging.LogScopeEnd();
	}

	private void HandleWindowResized()
	{
		lock (Window)
		{
			Camera.WindowResized(Window.Width, Window.Height);

			lock (swapchain)
			{
				swapchain.Resize(Window.Width, Window.Height);
			}

			GraphicsDevice.ResizeMainWindow(Window.Width, Window.Height);

			WindowResized?.Invoke(((int)Window.Width, (int)Window.Height));
		}
	}

	public void Run()
	{
		Debug.Assert(setUp, "The GraphicsWorld has not been set up!");

		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();

			TimeUnit timeLastUpdate = stopwatch.Elapsed.TotalSeconds;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (Window.Exists)
			{
				time = stopwatch.Elapsed.TotalSeconds;

				deltaTime = time - timeLastUpdate;

				CollectRenderingPerformanceData?.Invoke(deltaTime);

#if SilenceWeirdErrors
				try
				{
#endif
					Window.PumpEvents();
#if SilenceWeirdErrors
				}
				catch (Exception ex)
				{
					Logging.LogError($"An exception occured (and was swiftly silenced) while pumping window events. Weird, innit? Here it is: {ex}");
				}
#endif

				RenderScene(FixedDecimalLong8.Max((DecimalNumber)deltaTime, (DecimalNumber)FrametimeLowerLimit));

				Time.ResponseSpinner(time, ref responseSpinner);

				if (ShouldLimitFramerate)
				{
					Time.WaitIntervalLimit(FrametimeLowerLimit, time, stopwatch, out var timeOfContinuation);

					timeLastUpdate = timeOfContinuation;
				}
				else
				{
					timeLastUpdate = time;
				}

				FrameRendered?.Invoke();
			}
		}))
		{ Name = "Render Thread" };

		thread.Start();
	}

	private void CreateDeviceObjects(GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.graphicsDevice = gd;
		this.factory = factory;
		this.swapchain = swapchain;

		commandList = factory.CreateCommandList();

		cameraProjViewBuffer = factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		lightInfoBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		lightDir = Vector3FixedDecimalInt4.Normalize(new Vector3FixedDecimalInt4((FixedDecimalInt4)0.3, (FixedDecimalInt4)0.75, -(FixedDecimalInt4)0.3));

		viewInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MatrixPair>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
	}

	public void AddRenderable(IRenderable renderable)
	{
		Debug.Assert(renderable is not null);

		lock (unorderedRenderables)
		{
			unorderedRenderables.Add(renderable);
		}
	}

	public void AddRenderable(IRenderable renderable, int order)
	{
		Debug.Assert(renderable is not null);

		lock (specificOrderRenderables)
		{
			if (specificOrderRenderables.ContainsKey(order))
			{
				specificOrderRenderables[order].Add(renderable);

				renderableToOrder.AddUnique(renderable, order);
			}
			else
			{
				specificOrderRenderables.Add(order, new() { renderable });

				renderableToOrder.AddUnique(renderable, order);
			}
		}
	}

	public void RemoveRenderable(IRenderable renderable)
	{
		Debug.Assert(renderable is not null);

		lock (unorderedRenderables)
		{
			if (unorderedRenderables.Contains(renderable))
			{
				unorderedRenderables.Remove(renderable);
			}
			else if (renderableToOrder.ContainsKey(renderable))
			{
				specificOrderRenderables[renderableToOrder[renderable]].Remove(renderable);

				renderableToOrder.RemoveStrict(renderable);
			}
		}
	}

	private void RenderScene(FixedDecimalLong8 deltaTime) // Use FixedDecimalLong8 to make code simpler and faster, otherwise Debug would be too much slower.
	{
		lock (commandList)
		{
			// Begin() must be called before commands can be issued.
			commandList.Begin();

			Camera.UpdatePerspectiveMatrix();
			Camera.UpdateViewMatrix();

			// Update per-frame resources.
			commandList.UpdateBuffer(CameraProjViewBuffer, 0, new MatrixPair(Camera.ViewMatrix.ToMatrix4x4(), Camera.ProjectionMatrix.ToMatrix4x4()));

			commandList.UpdateBuffer(lightInfoBuffer, 0, new LightInfo(lightDir.ToVector3(), Camera.Transform.Position.ToVector3()));

			Matrix4x4.Invert(Camera.ProjectionMatrix.ToMatrix4x4(), out Matrix4x4 inverseProjection);
			Matrix4x4.Invert(Camera.ViewMatrix.ToMatrix4x4(), out Matrix4x4 inverseView);
			commandList.UpdateBuffer(viewInfoBuffer, 0, new MatrixPair(
				inverseProjection,
				inverseView));

			lock (swapchain)
			{
				// We want to render directly to the output Window.
				commandList.SetFramebuffer(swapchain.Framebuffer);
			}
			commandList.ClearColorTarget(0, RgbaFloat.Pink);
			commandList.ClearDepthStencil(1f);

			commandList.PushDebugGroup("Draw renderables");
			if (specificOrderRenderables.Count == 0)
			{
				lock (unorderedRenderables)
				{
					foreach (var renderable in unorderedRenderables)
					{
						renderable.AddDrawCommands(commandList, deltaTime);
					}
				}
			}
			else
			{
				bool hasRenderedUnorderedRenderables = false;
				lock (specificOrderRenderables)
				{
					foreach (var index in specificOrderRenderables.Keys)
					{
						if (index >= 0 && !hasRenderedUnorderedRenderables)
						{
							lock (unorderedRenderables)
							{
								foreach (var renderable in unorderedRenderables)
								{
									renderable.AddDrawCommands(commandList, deltaTime);

									hasRenderedUnorderedRenderables = true;
								}
							}
						}

						foreach (var renderable in specificOrderRenderables[index])
						{
							renderable.AddDrawCommands(commandList, deltaTime);
						}
					}
				}

				lock (unorderedRenderables)
				{
					if (!hasRenderedUnorderedRenderables)
					{
						foreach (var renderable in unorderedRenderables)
						{
							renderable.AddDrawCommands(commandList, deltaTime);
						}
					}
				}
			}
			commandList.PopDebugGroup();

			CustomDrawOperations?.Invoke(commandList);

			// End() must be called before commands can be submitted for execution.
			commandList.End();
		}

		GraphicsDevice.SubmitCommands(commandList);
		GraphicsDevice.WaitForIdle();

		// Once commands have been submitted, the rendered image can be presented to the application Window.
		lock (swapchain)
		{
			GraphicsDevice.SwapBuffers(swapchain);
		}
	}

	/// <summary>
	/// Works the same as calling <c>RemoveRenderable</c> on each renderable, just much faster.
	/// </summary>
	public void Reset()
	{
		Logging.LogDebug($"Resetting the {nameof(GraphicsWorld)}.");

		// Locks all objects which are lockable to ensure no activity is going on during the reset.
		lock (commandList)
		lock (unorderedRenderables)
		lock (specificOrderRenderables)
		{
			unorderedRenderables.Clear();
			specificOrderRenderables.Clear();
			renderableToOrder.Clear();

			foreach (var batchRenderable in BatchRenderable.BatchRenderables)
			{
				batchRenderable.Clear();
			}

			CustomDrawOperations = null;
			FrameRendered = null;
			CollectRenderingPerformanceData = null;
			WindowResized = null;
		}
	}
}
