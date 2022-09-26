using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Veldrid;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Game
{
	public class Player : IDisposable
	{
		public Transform Transform = Transform.Identity;

		private MainGame mainGame;

		private PhysicsWorld physicsWorld;

		private GraphicsWorld graphicsWorld;

		private GameWorld gameWorld;

		private UI ui;

		private ConstructionMarker constructionMarker;

		public Transform CameraTransform => new(Transform.Position, QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(FixedDecimalInt4.Zero, LookPitch, FixedDecimalInt4.Zero), Transform.Rotation).NormalizeQuaternion());

		private Player(MainGame mainGame, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, UI ui)
		{
			this.mainGame = mainGame;
			this.physicsWorld = physicsWorld;
			this.graphicsWorld = graphicsWorld;
			this.gameWorld = gameWorld;
			this.ui = ui;
		}

		public static Player Create(MainGame mainGame, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, UI ui)
		{
			Player player = new(mainGame, physicsWorld, graphicsWorld, gameWorld, ui);

			player.constructionMarker = ConstructionMarker.Create(graphicsWorld);

			return player;
		}

		public FixedDecimalLong8 RotationSnapping => 45 * FixedDecimalLong8.DegreesToRadians;

		public FixedDecimalLong8 RotationSnapped => ui.RotationIndex * RotationSnapping;

		public FixedDecimalInt4 LookPitch;

		public void Update(FixedDecimalInt4 deltaTime)
		{
			var lookedAtPhysicsObject = physicsWorld.Raycast(CameraTransform.Position, -CameraTransform.LocalUnitZ, 1000);

			if (lookedAtPhysicsObject is not null)
			{
				ui.CurrentlySelectedInformationProvider = lookedAtPhysicsObject.InformationProvider;
			}
			else
			{
				ui.CurrentlySelectedInformationProvider = null;
			}

			if (lookedAtPhysicsObject is not null)
			{
				if (InputTracker.GetKeyDown(Key.F))
				{
					lookedAtPhysicsObject.Entity.Interacted();
				}
			}

			if (ShouldShowConstructionMarker(lookedAtPhysicsObject))
			{
				PipeConnector pipeConnector = lookedAtPhysicsObject.Entity is Connector con ? (PipeConnector)con : (PipeConnector)(((InformationProxy)lookedAtPhysicsObject.Entity).ProxiedEntity);

				constructionMarker.SetMesh(ui.SelectedPipeType.Mesh);

				constructionMarker.SetColor(RgbaFloat.Green);

				constructionMarker.SetTransform(GameWorld.GenerateTransformForConnector(ui.SelectedPipeType.ConnectorPlacements[ui.ConnectorSelection], pipeConnector, RotationSnapped));

				constructionMarker.ShouldDraw = true;

				if (InputTracker.GetMouseButtonDown(MouseButton.Left))
				{
					Pipe.Build(pipeConnector, ui.SelectedPipeType, ui.ConnectorSelection, RotationSnapped, ui, physicsWorld, graphicsWorld, gameWorld, mainGame, gameWorld.SerializationReferenceHandler);

					constructionMarker.ShouldDraw = false;
				}
			}
			else if (lookedAtPhysicsObject is not null && lookedAtPhysicsObject.Entity is IConstruction construction)
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

			if (!ShouldShowConstructionMarker(lookedAtPhysicsObject))
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
				motionDir = Vector3FixedDecimalInt4.Transform(motionDir, Transform.Rotation);
				Transform.Position += motionDir * sprintFactor * deltaTime;
			}

			FixedDecimalInt4 yawDelta = -InputTracker.MouseDelta.X / 300;
			FixedDecimalInt4 pitchDelta = -InputTracker.MouseDelta.Y / 300;

			LookPitch += pitchDelta;

			LookPitch = FixedDecimalInt4.Clamp(LookPitch, -80 * FixedDecimalInt4.DegreesToRadians, 80 * FixedDecimalInt4.DegreesToRadians);

			Transform.Rotation = QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(yawDelta, FixedDecimalInt4.Zero, FixedDecimalInt4.Zero), Transform.Rotation).NormalizeQuaternion();
			//Transform.Rotation = QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(FixedDecimalInt4.Zero, LookPitch, FixedDecimalInt4.Zero), Transform.Rotation).NormalizeQuaternion();
		}

		private bool ShouldShowConstructionMarker(PhysicsObject lookedAtPhysicsObject)
		{
			return lookedAtPhysicsObject is not null && (((lookedAtPhysicsObject.Entity is Connector connector && (connector).Vacant) || (lookedAtPhysicsObject.Entity is InformationProxy informationProxy && informationProxy.ProxiedEntity is Connector proxiedConnector && proxiedConnector.Vacant))) && ui.SelectedPipeType is not null;
		}

		public bool Disposed = false;

		public void Dispose()
		{
			if (Disposed)
				return;

			constructionMarker.Destroy();
		}

		public void Serialize(XmlWriter writer)
		{
			writer.WriteStartElement("Player");
			{
				Transform.Serialize(writer);

				LookPitch.Serialize(writer);
			}
			writer.WriteEndElement();
		}

		public static Player Deserialize(XmlReader reader, MainGame mainGame, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, UI ui)
		{
			Player player = Create(mainGame, physicsWorld, graphicsWorld, gameWorld, ui);

			reader.ReadStartElement("Player");
			{
				player.Transform = reader.DeserializeTransform();

				player.LookPitch = reader.DeserializeFixedDecimalInt4();
			}
			reader.ReadEndElement();

			return player;
		}
	}
}
