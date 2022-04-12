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
	public GameWorld GameWorld;

	public static DebugRender DebugRender;

	public static DebugSettings DebugSettings = new();

	private Window window;
	private UI ui;

	public bool Paused;

	public String SynchronizationObject = "69";

	private Vector2FixedDecimalInt4 previousMousePos;

	private ConstructionMarker constructionMarker;

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

		ui.PauseStateChanged += UI_PauseStateChanged;

		GameWorld = new();

		Starfield.Create(GraphicsWorld);

		constructionMarker = ConstructionMarker.Create(GraphicsWorld);

		GameWorld.AddConstruction(Pipe.Create(ui.SelectedPipeType, new Transform(new(0, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), PhysicsWorld, GraphicsWorld));

		InputTracker.IgnoreNextFrameMousePosition = true;

		GraphicsWorld.Run();

		StartUpdating();
	}

	private void UI_PauseStateChanged(bool paused)
	{
		InputTracker.IgnoreNextFrameMousePosition = true;

		Paused = paused;

		window.CaptureMouse = !paused;
	}

	private void StartUpdating()
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

				Thread.Sleep((Time.UpdateInterval * 1000).ToInt32());

				Update(deltaTime);
			}
		}));

		thread.Start();
	}

	private void Update(FixedDecimalInt4 deltaTime)
	{
		lock(SynchronizationObject)
		{
			window.PumpEvents(out var input);

			InputTracker.UpdateFrameInput(input);

			ui.Update();

			if (InputTracker.GetKeyDown(Key.P))
			{
				DebugRender.ShouldRender = !DebugRender.ShouldRender;
			}

			if (!Paused)
			{
				var physicsObject = PhysicsWorld.Raycast(GraphicsWorld.Camera.Position, GraphicsWorld.Camera.Forward, 1000);

				if (physicsObject is not null)
				{
					ui.CurrentlySelectedInformationProvider = physicsObject.InformationProvider;
				}
				else
				{
					ui.CurrentlySelectedInformationProvider = null;
				}

				if (physicsObject is not null && (((physicsObject.Entity is Connector connector && ((PipeConnector)connector).Vacant) || (physicsObject.Entity is ConnectorProxy connectorProxy && ((PipeConnector)connectorProxy.Connector).Vacant))) && ui.SelectedPipeType is not null)
				{
					PipeConnector pipeConnector = physicsObject.Entity is Connector con ? (PipeConnector)con : ((PipeConnector)((ConnectorProxy)physicsObject.Entity).Connector);

					constructionMarker.SetMesh(ui.SelectedPipeType.Mesh);

					constructionMarker.SetColor(RgbaFloat.Green);

					constructionMarker.SetTransform(GameWorld.GenerateTransformForConnector(ui.SelectedPipeType.ConnectorPlacements[ui.ConnectorSelection], pipeConnector, ui.Rotation));

					constructionMarker.ShouldDraw = true;

					if (InputTracker.GetMouseButtonDown(MouseButton.Left))
					{
						GameWorld.AddConstruction(Pipe.Build(pipeConnector, ui.SelectedPipeType, ui.ConnectorSelection, ui.Rotation, PhysicsWorld, GraphicsWorld));

						constructionMarker.ShouldDraw = false;
					}
				}
				else if (physicsObject is not null && physicsObject.Entity is IConstruction construction)
				{
					if (InputTracker.GetMouseButtonDown(MouseButton.Right))
					{
						GameWorld.Deconstruct(construction);
					}
				}

				if (physicsObject is null || physicsObject.Entity is not Connector)
				{
					constructionMarker.ShouldDraw = false;
				}

				FixedDecimalInt4 sprintFactor = InputTracker.GetKey(Key.ShiftLeft)
									? 3
									: (FixedDecimalInt4)0.5f;
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
				GraphicsWorld.Camera.Pitch = FixedDecimalInt4.Clamp(GraphicsWorld.Camera.Pitch, -(FixedDecimalInt4)1.2f, (FixedDecimalInt4)1.2f);
			}
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
