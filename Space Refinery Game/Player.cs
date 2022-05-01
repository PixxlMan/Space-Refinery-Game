using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	public class Player
	{
		private Camera camera;

		private MainGame mainGame;

		private PhysicsWorld physicsWorld;

		private GraphicsWorld graphicsWorld;

		private GameWorld gameWorld;

		private UI ui;

		private ConstructionMarker constructionMarker;

		private Vector2FixedDecimalInt4 previousMousePos;

		public Player(Camera camera, MainGame mainGame, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, UI ui)
		{
			this.camera = camera;
			this.mainGame = mainGame;
			this.physicsWorld = physicsWorld;
			this.graphicsWorld = graphicsWorld;
			this.gameWorld = gameWorld;
			this.ui = ui;
		}

		public static Player Create(Camera camera, MainGame mainGame, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, UI ui)
		{
			Player player = new(camera, mainGame, physicsWorld, graphicsWorld, gameWorld, ui);

			player.constructionMarker = ConstructionMarker.Create(graphicsWorld);

			return player;
		}

		public FixedDecimalLong8 RotationSnapping => 45 * FixedDecimalLong8.DegreesToRadians;

		public FixedDecimalLong8 RotationSnapped => ui.RotationIndex * RotationSnapping;

		public void Update(FixedDecimalInt4 deltaTime)
		{
			var physicsObject = physicsWorld.Raycast(camera.Position, camera.Forward, 1000);

			if (physicsObject is not null)
			{
				ui.CurrentlySelectedInformationProvider = physicsObject.InformationProvider;
			}
			else
			{
				ui.CurrentlySelectedInformationProvider = null;
			}

			if (physicsObject is not null)
			{
				if (InputTracker.GetKeyDown(Key.F))
				{
					physicsObject.Entity.Interacted();
				}
			}

			if (physicsObject is not null && (((physicsObject.Entity is Connector connector && ((PipeConnector)connector).Vacant) || (physicsObject.Entity is ConnectorProxy connectorProxy && ((PipeConnector)connectorProxy.Connector).Vacant))) && ui.SelectedPipeType is not null)
			{
				PipeConnector pipeConnector = physicsObject.Entity is Connector con ? (PipeConnector)con : ((PipeConnector)((ConnectorProxy)physicsObject.Entity).Connector);

				constructionMarker.SetMesh(ui.SelectedPipeType.Mesh);

				constructionMarker.SetColor(RgbaFloat.Green);

				constructionMarker.SetTransform(GameWorld.GenerateTransformForConnector(ui.SelectedPipeType.ConnectorPlacements[ui.ConnectorSelection], pipeConnector, RotationSnapped));

				constructionMarker.ShouldDraw = true;

				if (InputTracker.GetMouseButtonDown(MouseButton.Left))
				{
					gameWorld.AddConstruction(Pipe.Build(pipeConnector, ui.SelectedPipeType, ui.ConnectorSelection, RotationSnapped, ui, physicsWorld, graphicsWorld, gameWorld, mainGame));

					constructionMarker.ShouldDraw = false;
				}
			}
			else if (physicsObject is not null && physicsObject.Entity is IConstruction construction)
			{
				if (construction is OrdinaryPipe pipe)
				{
					if (InputTracker.GetKeyDown(Key.U))
					{
						pipe.ResourceContainer.AddResource(new(mainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType, 10, 100 * mainGame.ChemicalTypesDictionary["Water"].LiquidPhaseType.SpecificHeatCapacity * 10, 0));
					}
				}

				if (InputTracker.GetMouseButtonDown(MouseButton.Right))
				{
					gameWorld.Deconstruct(construction);
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
				QuaternionFixedDecimalInt4 lookRotation = QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(graphicsWorld.Camera.Yaw, graphicsWorld.Camera.Pitch, FixedDecimalInt4.Zero);
				motionDir = Vector3FixedDecimalInt4.Transform(motionDir, lookRotation);
				graphicsWorld.Camera.Position += (motionDir * sprintFactor * deltaTime).ToVector3().ToFixed<Vector3FixedDecimalInt4>();
			}

			Vector2FixedDecimalInt4 mouseDelta = InputTracker.MousePosition - previousMousePos;
			previousMousePos = InputTracker.MousePosition;

			graphicsWorld.Camera.Yaw += -mouseDelta.X / 300;
			graphicsWorld.Camera.Pitch += -mouseDelta.Y / 300;
			graphicsWorld.Camera.Pitch = FixedDecimalInt4.Clamp(graphicsWorld.Camera.Pitch, -(FixedDecimalInt4)1.2f, (FixedDecimalInt4)1.2f);
		}
	}
}
