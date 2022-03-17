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
using BepuPhysics;
using BepuUtilities.Memory;

namespace Space_Refinery_Game;

public class MainGame
{
	public GraphicsWorld GraphicsWorld;
	public PhysicsWorld PhysicsWorld;

	private Window window;
	private UI ui;

	public String SynchronizationObject = "69";

	private Vector2FixedDecimalInt4 previousMousePos;

	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.window = window;

		ui = new(gd);

		GraphicsWorld = new();

		GraphicsWorld.SetUp(window, gd, factory, swapchain);

		PhysicsWorld = new();

		PhysicsWorld.SetUp();

		PhysicsWorld.Run();

		StartUpdating();

		GraphicsWorld.AddRenderable(ui);

		PipeStraght.Create(PhysicsWorld, GraphicsWorld, new Transform(new(0, 2, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(90 * FixedDecimalInt4.DegreesToRadians, 0, 0)));

		GraphicsWorld.Run();
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

	FixedDecimalInt4 flow = 0;
	private void Update(FixedDecimalInt4 deltaTime)
	{
		lock(SynchronizationObject)
		{
			InputTracker.UpdateFrameInput(window.PumpEvents());

			if (InputTracker.GetKey(Key.Escape))
			{
				Environment.Exit(69);
			}

			flow += deltaTime * FixedDecimalInt4.DegreesToRadians * 10;
			((ITransformable)(GraphicsWorld.SceneRenderables[3])).Rotation = QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(flow, -90 * FixedDecimalInt4.DegreesToRadians, 0);

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
				QuaternionFixedDecimalInt4 lookRotation = QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(GraphicsWorld.Camera.Yaw, GraphicsWorld.Camera.Pitch, FixedDecimalInt4.Zero);
				motionDir = Vector3FixedDecimalInt4.Transform(motionDir, lookRotation);
				GraphicsWorld.Camera.Position += (motionDir * sprintFactor * deltaTime).ToVector3().ToFixed<Vector3FixedDecimalInt4>();
			}

			Vector2FixedDecimalInt4 mouseDelta = InputTracker.MousePosition - previousMousePos;
			previousMousePos = InputTracker.MousePosition;

			GraphicsWorld.Camera.Yaw += -mouseDelta.X / 300;
			GraphicsWorld.Camera.Pitch += -mouseDelta.Y / 300;
			GraphicsWorld.Camera.Pitch = FixedDecimalInt4.Clamp(GraphicsWorld.Camera.Pitch, -1, 1);
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
