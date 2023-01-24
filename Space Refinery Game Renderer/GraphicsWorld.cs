﻿using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Concurrent;
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

public sealed class GraphicsWorld
{
	private HashSet<IRenderable> unorderedRenderables = new();

	private SortedDictionary<int, HashSet<IRenderable>> specificOrderRenderables = new(); // Use Lookup<int, IRenderable> and sort manually as it's not very common to add objects?

	private ConcurrentDictionary<IRenderable, int> renderableToOrder = new();

	public GraphicsDevice GraphicsDevice;

	public ResourceFactory Factory;

	public Swapchain Swapchain;

	private CommandList commandList;

	public DeviceBuffer CameraProjViewBuffer;

	public DeviceBuffer LightInfoBuffer;

	private Vector3FixedDecimalInt4 lightDir;

	public DeviceBuffer ViewInfoBuffer;

	private Window window;

	public MeshLoader MeshLoader { get; private set; }

	public ShaderLoader ShaderLoader { get; private set; }

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock(responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	public readonly object SynchronizationObject = new();

	public Camera Camera;

	public FixedDecimalLong8 FrametimeLowerLimit = (FixedDecimalLong8)0.001;

	public bool ShouldLimitFramerate = true;

	public event Action<CommandList> CustomDrawOperations;

	public event Action FrameRendered;

	public event Action<FixedDecimalLong8> CollectRenderingPerformanceData;

	public event Action<int, int> WindowResized;

	public void SetUp(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.window = window;

		window.Resized += HandleWindowResized;

		Camera = new(window.Width, window.Height, Perspective.Perspective);

		Camera.Transform.Position = new Vector3FixedDecimalInt4(0, 0, 10);

		Camera.FarDistance = 10000;

		Camera.NearDistance = (FixedDecimalInt4)0.1;

		Camera.FieldOfView = 75 * FixedDecimalInt4.DegreesToRadians;

		MeshLoader = new(this);

		ShaderLoader = new(this);

		CreateDeviceObjects(gd, factory, swapchain);

		RenderingResources.CreateStaticDeviceResources(this);
	}

	private void HandleWindowResized()
	{
		Camera.WindowResized(window.Width, window.Height);

		lock (Swapchain) // is locking necessary for Swapchain?
		{
			Swapchain.Resize(window.Width, window.Height);
		}

		GraphicsDevice.ResizeMainWindow(window.Width, window.Height);

		WindowResized?.Invoke((int)window.Width, (int)window.Height);
	}

	public void Run()
	{
		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();

			FixedDecimalLong8 timeLastUpdate = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>();
			FixedDecimalLong8 time;
			FixedDecimalLong8 deltaTime;
			while (window.Exists)
			{
				time = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>();

				deltaTime = time - timeLastUpdate;

				CollectRenderingPerformanceData?.Invoke(deltaTime);

				window.PumpEvents();

				RenderScene(FixedDecimalLong8.Max(deltaTime, FrametimeLowerLimit));

				lock (responseSpinner)
					responseSpinner = Time.ResponseSpinner(time);

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
		this.GraphicsDevice = gd;
		this.Factory = factory;
		this.Swapchain = swapchain;

		commandList = factory.CreateCommandList();

		CameraProjViewBuffer = factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		LightInfoBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		lightDir = Vector3FixedDecimalInt4.Normalize(new Vector3FixedDecimalInt4((FixedDecimalInt4)0.3, (FixedDecimalInt4)0.75, -(FixedDecimalInt4)0.3));

		ViewInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MatrixPair>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
	}

	public void AddRenderable(IRenderable renderable)
	{
		lock (unorderedRenderables)
		{
			unorderedRenderables.Add(renderable);
		}
	}

	public void AddRenderable(IRenderable renderable, int order)
	{
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

	private void RenderScene(FixedDecimalLong8 deltaTime)
	{
		lock (commandList)
		{
			// Begin() must be called before commands can be issued.
			commandList.Begin();

			Camera.UpdatePerspectiveMatrix();
			Camera.UpdateViewMatrix();

			// Update per-frame resources.
			commandList.UpdateBuffer(CameraProjViewBuffer, 0, new MatrixPair(Camera.ViewMatrix.ToMatrix4x4(), Camera.ProjectionMatrix.ToMatrix4x4()));

			commandList.UpdateBuffer(LightInfoBuffer, 0, new LightInfo(lightDir.ToVector3(), Camera.Transform.Position.ToVector3()));

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

		// Once commands have been submitted, the rendered image can be presented to the application window.
		GraphicsDevice.SwapBuffers(Swapchain);
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
