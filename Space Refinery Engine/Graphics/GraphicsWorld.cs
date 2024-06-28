using FixedPrecision;
using SixLabors.ImageSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace Space_Refinery_Engine.Renderer;

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

	private List<IShadowCaster> shadowCasters = new();

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

	private readonly object renderFramebufferSyncRoot = new();
	private Framebuffer renderFramebuffer;
	public Framebuffer RenderFramebuffer
	{
		get
		{
			lock (renderFramebufferSyncRoot)
			{
				return renderFramebuffer;
			}
		}

		set
		{
			lock (renderFramebufferSyncRoot)
			{
				renderFramebuffer = value;
			}
		}
	}

	private readonly object shadowFramebufferSyncRoot = new();
	private Framebuffer shadowFramebuffer;
	public Framebuffer ShadowFramebuffer
	{
		get
		{
			lock (shadowFramebufferSyncRoot)
			{
				return shadowFramebuffer;
			}
		}

		set
		{
			lock (shadowFramebufferSyncRoot)
			{
				shadowFramebuffer = value;
			}
		}
	}
	private Texture shadowMapTexture;

	public OutputDescription RenderingOutputDescription => RenderFramebuffer.OutputDescription;
	public OutputDescription ShadowRenderingOutputDescription => ShadowFramebuffer.OutputDescription;

	private CommandList commandList;

	private Vector3FixedDecimalInt4 lightDir;

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
	/// <summary>
	/// The response spinner can be used to visually show in the UI that the thread is running correctly and is not stopped or deadlocked.
	/// </summary>
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } }

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

	private FullscreenQuad fullscreenQuad;

	public IntervalUnit FrametimeLowerLimit { get; set; } = 0.001;

	public bool ShouldLimitFramerate = true;

	public event Action<CommandList>? CustomDrawOperations;

	public event Action? FrameRendered;

	public event Action<IntervalUnit>? CollectRenderingPerformanceData;

	public event Action<int, int>? WindowResized;

	public void SetUp(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		Debug.Assert(!setUp, "The GraphicsWorld has already been set up!");

		Logging.LogScopeStart("Setting up GraphicsWorld");

		// No dependency
		Configuration.Default.PreferContiguousImageBuffers = true; // Use contigous image buffers in ImageSharp to load textures.
																   // This is necessary for them to uploadable to the GPU!
																   // No dependency
		ShaderLoader = new(this);


		// No dependency
		this.window = window;

		// No dependency
		Window.Resized += HandleWindowResized;

		// No dependency
		lightDir = Vector3FixedDecimalInt4.Normalize(new Vector3FixedDecimalInt4((FixedDecimalInt4)0.3, (FixedDecimalInt4)0.75, -(FixedDecimalInt4)0.3));


		// Depends on Window
		camera = new(window.Width, window.Height, Perspective.Perspective);
		Camera.FarDistance = 100;
		Camera.NearDistance = 0.1;
		Camera.FieldOfView = 75 * FixedDecimalInt4.DegreesToRadians;


		// These depend on the shader loader
		CreateDeviceObjects(gd, factory, swapchain);
		RenderingResources.CreateStaticDeviceResources(this);

		// These depend on rendering resources
		fullscreenQuad = new();
		fullscreenQuad.CreateDeviceObject(this);

		// These depend on device resources.
		MeshLoader = new(this);

		// This depends on using contigous image buffers.
		MaterialLoader = new(this);

		setUp = true;

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

			CreateRenderFramebuffer();

			GraphicsDevice.ResizeMainWindow(Window.Width, Window.Height);

			WindowResized?.Invoke((int)Window.Width, (int)Window.Height);
		}
	}

	private void CreateRenderFramebuffer()
	{
		lock (renderFramebufferSyncRoot)
		{
			RenderFramebuffer?.Dispose();

			Texture depthTexture = Factory.CreateTexture(TextureDescription.Texture2D(Window.Width, Window.Height, 1, 1, RenderingResources.DepthFormat, TextureUsage.DepthStencil | TextureUsage.Sampled));
			depthTexture.Name = "Render framebuffer depth texture";
			Texture colorTexture = Factory.CreateTexture(TextureDescription.Texture2D(Window.Width, Window.Height, 1, 1, RenderingResources.InternalColorFormat, TextureUsage.RenderTarget | TextureUsage.Sampled));
			colorTexture.Name = "Render framebuffer color texture";
			var framebuffer = Factory.CreateFramebuffer(new(depthTexture, colorTexture));
			framebuffer.Name = "Render framebuffer";

			RenderFramebuffer = framebuffer;
		}
	}

	private void CreateShadowMapFramebuffer()
	{
		lock (shadowFramebufferSyncRoot)
		{
			ShadowFramebuffer?.Dispose();

			shadowMapTexture = Factory.CreateTexture(TextureDescription.Texture2D(4096, 4096, 1, 1, RenderingResources.DepthFormat, TextureUsage.DepthStencil | TextureUsage.Sampled));
			shadowMapTexture.Name = "Shadow map framebuffer depth texture";
			var framebuffer = Factory.CreateFramebuffer(new(shadowMapTexture, []));
			framebuffer.Name = "Shadow map framebuffer";

			ShadowFramebuffer = framebuffer;
		}
	}

	// https://gamedev.stackexchange.com/questions/193929/how-to-move-the-shadow-map-with-the-camera
	// https://learnopengl.com/code_viewer_gh.php?code=src/8.guest/2021/2.csm/shadow_mapping.cpp
	// https://learnopengl.com/Guest-Articles/2021/CSM
	public void CalculateShadowMapMatricies(out Matrix4x4FixedDecimalInt4 lightViewMatrix, out Matrix4x4FixedDecimalInt4 lightProjectionMatrix)
	{
		FixedDecimalInt4 Hnear = 2 * FixedDecimalInt4.Tan(camera.FieldOfView / 2) * camera.NearDistance;
		FixedDecimalInt4 Wnear = Hnear * camera.AspectRatio;
		FixedDecimalInt4 Hfar = 2 * FixedDecimalInt4.Tan(camera.FieldOfView / 2) * camera.FarDistance;
		FixedDecimalInt4 Wfar = Hfar * camera.AspectRatio;

		Vector3FixedDecimalInt4 centerFar = camera.Transform.Position + camera.Forward * camera.FarDistance;
		Vector3FixedDecimalInt4 centerNear = camera.Transform.Position + camera.Forward * camera.NearDistance;
		Vector3FixedDecimalInt4 frustumCenter = (centerFar - centerNear) * 0.5;

		Vector3FixedDecimalInt4 topLeftFar = centerFar + (camera.Transform.LocalUnitY * Hfar / 2) - (camera.Transform.LocalUnitX * Wfar / 2);
		Vector3FixedDecimalInt4 topRightFar = centerFar + (camera.Transform.LocalUnitY * Hfar / 2) + (camera.Transform.LocalUnitX * Wfar / 2);
		Vector3FixedDecimalInt4 bottomLeftFar = centerFar - (camera.Transform.LocalUnitY * Hfar / 2) - (camera.Transform.LocalUnitX * Wfar / 2);
		Vector3FixedDecimalInt4 bottomRightFar = centerFar - (camera.Transform.LocalUnitY * Hfar / 2) + (camera.Transform.LocalUnitX * Wfar / 2);

		Vector3FixedDecimalInt4 topLeftNear = centerNear + (camera.Transform.LocalUnitY * Hnear / 2) - (camera.Transform.LocalUnitX * Wnear / 2);
		Vector3FixedDecimalInt4 topRightNear = centerNear + (camera.Transform.LocalUnitY * Hnear / 2) + (camera.Transform.LocalUnitX * Wnear / 2);
		Vector3FixedDecimalInt4 bottomLeftNear = centerNear - (camera.Transform.LocalUnitY * Hnear / 2) - (camera.Transform.LocalUnitX * Wnear / 2);
		Vector3FixedDecimalInt4 bottomRightNear = centerNear - (camera.Transform.LocalUnitY * Hnear / 2) + (camera.Transform.LocalUnitX * Wnear / 2);

		Vector3FixedDecimalInt4[] corners = [topLeftFar, topRightFar, bottomLeftFar, bottomRightFar, topLeftNear, topRightNear, bottomLeftNear, bottomRightNear];

		Matrix4x4FixedDecimalInt4 lightView = Matrix4x4FixedDecimalInt4.CreateLookAt(frustumCenter + lightDir, frustumCenter, new(0.0, 1.0, 0.0));

		FixedDecimalInt4 minX = FixedDecimalInt4.MaxValue;
		FixedDecimalInt4 maxX = -FixedDecimalInt4.MaxValue;
		FixedDecimalInt4 minY = FixedDecimalInt4.MaxValue;
		FixedDecimalInt4 maxY = -FixedDecimalInt4.MaxValue;
		FixedDecimalInt4 minZ = FixedDecimalInt4.MaxValue;
		FixedDecimalInt4 maxZ = - FixedDecimalInt4.MaxValue;
		foreach (var corner in corners)
		{
			var trf = LeftMultiplyColumnMajor(lightView, corner);
			minX = FixedDecimalInt4.Min(minX, trf.X);
			maxX = FixedDecimalInt4.Max(maxX, trf.X);
			minY = FixedDecimalInt4.Min(minY, trf.Y);
			maxY = FixedDecimalInt4.Max(maxY, trf.Y);
			minZ = FixedDecimalInt4.Min(minZ, trf.Z);
			maxZ = FixedDecimalInt4.Max(maxZ, trf.Z);
			
			if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Debug display shadow map"))
			{
				GameData.DebugRender.DrawCube(camera.Transform.PerformTransform(new(corner)), RgbaFloat.Red, new(0.1, 0.1, 0.1));
			}
		}

		// Tune this parameter according to the scene
		FixedDecimalInt4 zMult = 10.0;
		if (minZ < 0)
		{
			minZ *= zMult;
		}
		else
		{
			minZ /= zMult;
		}
		if (maxZ < 0)
		{
			maxZ /= zMult;
		}
		else
		{
			maxZ *= zMult;
		}

		lightProjectionMatrix = Matrix4x4FixedDecimalInt4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);

		lightViewMatrix = /*lightProjectionMatrix **/ lightView;

		if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Debug display shadow map"))
		{
			GameData.DebugRender.DrawCube(camera.Transform.PerformTransform(new(lightViewMatrix.ToMatrix4x4().Translation.ToFixed<Vector3FixedDecimalInt4>())), RgbaFloat.White, Vector3FixedDecimalInt4.One * 10);
			Logging.Log(lightViewMatrix.ToMatrix4x4().Translation.ToString());
		}

		// https://stackoverflow.com/questions/7574125/multiplying-a-matrix-and-a-vector-in-glm-opengl
		static Vector3FixedDecimalInt4 LeftMultiplyColumnMajor(Matrix4x4FixedDecimalInt4 matrix, Vector3FixedDecimalInt4 vector)
		{
			return new(
				matrix.M11 * vector.X,
				matrix.M12 * vector.Y,
				matrix.M13 * vector.Z
				);
		}
	}

	public void Run()
	{
		Debug.Assert(setUp, "The GraphicsWorld has not been set up!");

		Thread thread = new(new ThreadStart(() =>
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

		CreateRenderFramebuffer();
		CreateShadowMapFramebuffer();

		commandList = factory.CreateCommandList();
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
				specificOrderRenderables.Add(order, [renderable]);

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

	public void AddShadowCaster(IShadowCaster shadowCaster)
	{
		Debug.Assert(shadowCaster is not null);

		lock (shadowCasters)
		{
			shadowCasters.Add(shadowCaster);
		}
	}

	public void RemoveShadowCaster(IShadowCaster shadowCaster)
	{
		Debug.Assert(shadowCaster is not null);

		lock (shadowCasters)
		{
			shadowCasters.Remove(shadowCaster);
		}
	}

	private void RenderScene(FixedDecimalLong8 deltaTime) // Use FixedDecimalLong8 to make code simpler and faster, otherwise Debug would be too much slower.
	{
		lock (commandList)
		{
			// Begin() must be called before commands can be issued.
			commandList.Begin();

			commandList.PushDebugGroup("Updating resources");
			Camera.UpdatePerspectiveMatrix();
			Camera.UpdateViewMatrix();

			commandList.UpdateBuffer(RenderingResources.CameraProjViewBuffer, 0, new MatrixPair(Camera.ViewMatrix.ToMatrix4x4(), Camera.ProjectionMatrix.ToMatrix4x4()));
			commandList.UpdateBuffer(RenderingResources.LightInfoBuffer, 0, new LightInfo(lightDir.ToVector3(), Camera.Transform.Position.ToVector3()));

			Matrix4x4.Invert(Camera.ProjectionMatrix.ToMatrix4x4(), out Matrix4x4 inverseProjection);
			Matrix4x4.Invert(Camera.ViewMatrix.ToMatrix4x4(), out Matrix4x4 inverseView);
			commandList.UpdateBuffer(RenderingResources.ViewInfoBuffer, 0, new MatrixPair(
				inverseProjection,
				inverseView));
			commandList.PopDebugGroup();

			commandList.PushDebugGroup("Draw shadow casters");
			commandList.SetFramebuffer(ShadowFramebuffer);
			commandList.ClearDepthStencil(1f);

			CalculateShadowMapMatricies(out var lightViewMatrix, out var lightProjectionMatrix);
			commandList.UpdateBuffer(RenderingResources.ShadowProjViewBuffer, 0, new MatrixPair(lightViewMatrix.ToMatrix4x4(), lightProjectionMatrix.ToMatrix4x4()));

			foreach (var shadowCaster in shadowCasters)
			{
				shadowCaster.AddShadowCasterDrawCommands(commandList);
			}

			commandList.PopDebugGroup();

			commandList.SetFramebuffer(RenderFramebuffer);
			commandList.PushDebugGroup("Draw renderables");
			commandList.ClearColorTarget(0, RgbaFloat.Pink);
			commandList.ClearDepthStencil(1f);

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

			commandList.PushDebugGroup("Custom draw operations");
			CustomDrawOperations?.Invoke(commandList);
			commandList.PopDebugGroup();

			// Direct3D 11 doesn't allow easy access to the device textures, so we render to a fullscreen quad instead.
			switch (graphicsDevice.BackendType)
			{
				case GraphicsBackend.Direct3D11:
					commandList.SetFramebuffer(swapchain.Framebuffer);
					fullscreenQuad.AddDrawCommands(commandList, deltaTime);
					break;
				case GraphicsBackend.Vulkan:
				case GraphicsBackend.OpenGL:
				case GraphicsBackend.Metal:
				case GraphicsBackend.OpenGLES:
					commandList.CopyTexture(renderFramebuffer.ColorTargets[0].Target, swapchain.Framebuffer.ColorTargets[0].Target);
					break;
			}

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
