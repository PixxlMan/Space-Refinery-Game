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
using static FixedPrecision.Convenience;
using BepuPhysics;
using BepuUtilities.Memory;

namespace Space_Refinery_Game;

public class MainGame
{
	public GraphicsWorld GraphicsWorld;
	public PhysicsWorld PhysicsWorld;

	public static DebugRender DebugRender;

	private Window window;
	private UI ui;

	public String SynchronizationObject = "69";

	private Vector2FixedDecimalInt4 previousMousePos;

	public void Start(Window window, GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
	{
		this.window = window;

		GraphicsWorld = new();

		GraphicsWorld.SetUp(window, gd, factory, swapchain);

		DebugRender = DebugRender.Create(GraphicsWorld);

		PhysicsWorld = new();

		PhysicsWorld.SetUp();

		PhysicsWorld.Run();

		ui = UI.Create(GraphicsWorld);

		Starfield.Create(GraphicsWorld);

		Pipe.Create(ui.SelectedPipeType, new Transform(new(5, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 45 * FixedDecimalInt4.DegreesToRadians)), PhysicsWorld, GraphicsWorld);

		Pipe.Create(ui.SelectedPipeType, new Transform(new(5, 0, 1), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), PhysicsWorld, GraphicsWorld);

		DebugRender.DrawRay(default, Vector3FixedDecimalInt4.UnitX, RgbaFloat.Red);
		DebugRender.DrawRay(default, Vector3FixedDecimalInt4.UnitY, RgbaFloat.Green);
		DebugRender.DrawRay(default, Vector3FixedDecimalInt4.UnitZ, RgbaFloat.Blue);

		GraphicsWorld.Run();

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

	private void Update(FixedDecimalInt4 deltaTime)
	{
		lock(SynchronizationObject)
		{
			InputTracker.UpdateFrameInput(window.PumpEvents());

			if (InputTracker.GetKey(Key.Escape))
			{
				Environment.Exit(69);
			}

			if (InputTracker.FrameSnapshot.WheelDelta != 0)
			{
				ui.ChangeSelection(-(int)InputTracker.FrameSnapshot.WheelDelta);
			}

			var physicsObject = PhysicsWorld.Raycast(GraphicsWorld.Camera.Position, GraphicsWorld.Camera.Forward, 1000);

			if (physicsObject is not null)
			{
				ui.CurrentlySelectedInformationProvider = physicsObject.InformationProvider;
			}
			else
			{
				ui.CurrentlySelectedInformationProvider = null;
			}

			if (physicsObject is not null && physicsObject.Entity is Connector && ui.SelectedPipeType is not null)
			{
				if (InputTracker.GetMouseButtonDown(MouseButton.Left))
				{
					Pipe.Build((Connector)physicsObject.Entity, ui.SelectedPipeType, 0, PhysicsWorld, GraphicsWorld);
				}
			}
			else if (physicsObject is not null && physicsObject.Entity is IConstruction)
			{
				if (InputTracker.GetMouseButtonDown(MouseButton.Right))
				{
					((IConstruction)physicsObject.Entity).Deconstruct();
				}
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
