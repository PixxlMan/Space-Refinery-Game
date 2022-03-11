﻿using FixedPrecision;
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

namespace Space_Refinery_Game;

public class MainGame
{
	public List<IRenderable> SceneRenderables = new();

	private GraphicsDevice gd;
	private ResourceFactory factory;
	private Swapchain swapchain;
	private CommandList commandList;
	private DeviceBuffer cameraProjViewBuffer;
	private DeviceBuffer lightInfoBuffer;
	private Vector3FixedDecimalInt4 lightDir;
	private DeviceBuffer viewInfoBuffer;
	private Window window;

	private Camera camera;

	private Vector2FixedDecimalInt4 previousMousePos;

	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		window.Rendering += (_) => RenderScene();

		window.Resized += () => camera.WindowResized(window.Width, window.Height);

		camera = new(window.Width, window.Height);

		camera.Position = new Vector3FixedDecimalInt4(0, 0, 10);
		camera.Pitch = "-0.3".Parse<FixedDecimalInt4>();
		camera.Yaw = "0.1".Parse<FixedDecimalInt4>();

		this.window = window;

		CreateGameObjects(gd, factory, swapchain);

		AddDefaultObjects();

		StartUpdating();
	}

	private void StartUpdating()
	{
		Thread thread = new Thread(new ParameterizedThreadStart((_) => 
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

				Thread.Sleep(6);

				Update(deltaTime);
			}
		}));

		thread.Start();
	}

	private void CreateGameObjects(GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.gd = gd;
		this.factory = factory;
		this.swapchain = swapchain;

		commandList = factory.CreateCommandList();

		cameraProjViewBuffer = factory.CreateBuffer(
			new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * 2), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		lightInfoBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		lightDir = Vector3FixedDecimalInt4.Normalize(new Vector3FixedDecimalInt4("0.3".Parse<FixedDecimalInt4>(), "-0.75".Parse<FixedDecimalInt4>(), "-0.3".Parse<FixedDecimalInt4>()));

		viewInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MatrixPair>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
	}

	private void AddDefaultObjects()
	{
		SceneRenderables.Add(StarfieldRenderable.Create(viewInfoBuffer, gd, factory));

		SceneRenderables.Add(EntityRenderable.Create(gd, factory, new Transform(new(0, 0, 0)), Mesh.LoadMesh(gd, factory, Path.Combine(Environment.CurrentDirectory, "Pipe_1mx1m_Straight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Red, gd, factory), cameraProjViewBuffer, lightInfoBuffer));

		SceneRenderables.Add(EntityRenderable.Create(gd, factory, new Transform(new(0, 2, 0)), Mesh.LoadMesh(gd, factory, Path.Combine(Environment.CurrentDirectory, "Pipe_1mx1m_Straight.obj")), Utils.GetSolidColoredTexture(RgbaByte.Red, gd, factory), cameraProjViewBuffer, lightInfoBuffer));
	}

	private void Update(FixedDecimalInt4 deltaTime)
	{
		InputTracker.UpdateFrameInput(window.PumpEvents());

		if (InputTracker.GetKey(Key.Escape))
		{
			Environment.Exit(69);
		}

		FixedDecimalInt4 sprintFactor = InputTracker.GetKey(Key.ShiftLeft)
							? 3
							: "0.5".Parse<FixedDecimalInt4>();
		Vector3FixedDecimalInt4 motionDir = Vector3FixedDecimalInt4.Zero;
		if (InputTracker.GetKey(Key.A))
		{
			motionDir += -Vector3FixedDecimalInt4.UnitX;
		}
		if (InputTracker.GetKey(Key.D))
		{
			motionDir += Vector3FixedDecimalInt4.UnitX;
		}
		if (InputTracker.GetKey(Key.W))
		{
			motionDir += -Vector3FixedDecimalInt4.UnitZ;
		}
		if (InputTracker.GetKey(Key.S))
		{
			motionDir += Vector3FixedDecimalInt4.UnitZ;
		}
		if (InputTracker.GetKey(Key.Q))
		{
			motionDir += -Vector3FixedDecimalInt4.UnitY;
		}
		if (InputTracker.GetKey(Key.E))
		{
			motionDir += Vector3FixedDecimalInt4.UnitY;
		}

		if (motionDir != Vector3FixedDecimalInt4.Zero)
		{
			QuaternionFixedDecimalInt4 lookRotation = QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(camera.Yaw.ToDouble().ToFixed<FixedDecimalInt4>(), camera.Pitch.ToDouble().ToFixed<FixedDecimalInt4>(), FixedDecimalInt4.Zero);
			motionDir = Vector3FixedDecimalInt4.Transform(motionDir, lookRotation);
			camera.Position += (motionDir * sprintFactor * deltaTime).ToVector3().ToFixed<Vector3FixedDecimalInt4>();
		}

		Vector2FixedDecimalInt4 mouseDelta = InputTracker.MousePosition - previousMousePos;
		previousMousePos = InputTracker.MousePosition;

		//if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
		{
			camera.Yaw += -mouseDelta.X / 300;
			camera.Pitch += -mouseDelta.Y / 300;
			camera.Pitch = camera.Clamp(camera.Pitch, -1, 1);
		}
	}

	private void RenderScene()
	{
		Thread.Sleep(1);

		// Begin() must be called before commands can be issued.
		commandList.Begin();

		// Update per-frame resources.
		commandList.UpdateBuffer(cameraProjViewBuffer, 0, new MatrixPair(camera.ViewMatrix.ToMatrix4x4(), camera.ProjectionMatrix.ToMatrix4x4()));

		commandList.UpdateBuffer(lightInfoBuffer, 0, new LightInfo(lightDir.ToVector3(), camera.Position.ToVector3()));

		Matrix4x4.Invert(camera.ProjectionMatrix.ToMatrix4x4(), out Matrix4x4 inverseProjection);
		Matrix4x4.Invert(camera.ViewMatrix.ToMatrix4x4(), out Matrix4x4 inverseView);
		commandList.UpdateBuffer(viewInfoBuffer, 0, new MatrixPair(
			inverseProjection,
			inverseView));

		// We want to render directly to the output window.
		commandList.SetFramebuffer(swapchain.Framebuffer);
		commandList.ClearColorTarget(0, RgbaFloat.White);
		commandList.ClearDepthStencil(1f);

		foreach (var renderable in SceneRenderables)
		{
			renderable.AddDrawCommands(commandList);
		}

		// End() must be called before commands can be submitted for execution.
		commandList.End();
		gd.SubmitCommands(commandList);
		gd.WaitForIdle();

		// Once commands have been submitted, the rendered image can be presented to the application window.
		gd.SwapBuffers(swapchain);
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
