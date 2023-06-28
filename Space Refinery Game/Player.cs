using BepuPhysics.Constraints;
using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using Space_Refinery_Utilities;
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
	public sealed class Player
	{
		public Transform Transform = Transform.Identity;

		private GameData gameData;

		private ConstructionMarker constructionMarker;

		public Transform CameraTransform => new(Transform.Position, QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(FixedDecimalInt4.Zero, LookPitch, FixedDecimalInt4.Zero), Transform.Rotation).NormalizeQuaternion());

		private Player(GameData gameData)
		{
			this.gameData = gameData;
		}

		public static Player Create(GameData gameData)
		{
			Player player = new(gameData)
			{
				constructionMarker = ConstructionMarker.Create(gameData.GraphicsWorld)
			};

			return player;
		}

		public FixedDecimalLong8 RotationSnapping => 45 * FixedDecimalLong8.DegreesToRadians;

		public FixedDecimalLong8 RotationSnapped => gameData.UI.RotationIndex * RotationSnapping;

		public FixedDecimalInt4 LookPitch;

		public void Update(FixedDecimalInt4 deltaTime)
		{
			var lookedAtPhysicsObject = gameData.PhysicsWorld.Raycast(CameraTransform.Position, -CameraTransform.LocalUnitZ, 1000);

			if (lookedAtPhysicsObject is not null)
			{
				gameData.UI.CurrentlySelectedInformationProvider = lookedAtPhysicsObject.InformationProvider;
			}
			else
			{
				gameData.UI.CurrentlySelectedInformationProvider = null;
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

				constructionMarker.SetMesh(gameData.UI.SelectedPipeType.Mesh);

				constructionMarker.SetTransform(GameWorld.GenerateTransformForConnector(gameData.UI.SelectedPipeType.ConnectorPlacements[gameData.UI.ConnectorSelection], pipeConnector, RotationSnapped));

				constructionMarker.ShouldDraw = true;

				if (Pipe.ValidateBuild(pipeConnector, gameData.UI.SelectedPipeType, gameData.UI.ConnectorSelection, RotationSnapped, gameData))
				{
					constructionMarker.State = ConstructionMarker.ConstructionMarkerState.LegalBuild;

					if (InputTracker.GetMouseButton(MouseButton.Left))
					{
						Pipe.Build(pipeConnector, gameData.UI.SelectedPipeType, gameData.UI.ConnectorSelection, RotationSnapped, gameData, gameData.ReferenceHandler);

						constructionMarker.ShouldDraw = false;
					}
				}
				else
				{
					constructionMarker.State = ConstructionMarker.ConstructionMarkerState.IllegalBuild;
				}
			}
			else if (lookedAtPhysicsObject is not null && lookedAtPhysicsObject.Entity is IConstruction construction)
			{
#if DEBUG
				if (InputTracker.GetKeyDown(Key.F10))
				{
					DebugStopPoints.RegisterStopPoint(construction.SerializableReference);
				}
#endif
				{
					if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Insert resource with button") && construction is OrdinaryPipe pipe)
					{
						ChemicalType chemicalType = MainGame.DebugSettings.AccessSetting<ComboDebugSetting<ChemicalType>>("Chemical type to insert with button", new(ChemicalType.ChemicalTypes.ToArray(), ChemicalType.Water));

						ChemicalPhase chemicalPhase = MainGame.DebugSettings.AccessSetting<EnumDebugSetting<ChemicalPhase>>("Chemical phase to insert with button", ChemicalPhase.Liquid);

						DecimalNumber mass = MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Mass to insert with button", new(10, 0, 1_000));

						ResourceType resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);

						DecimalNumber internalEnergy = ChemicalType.TemperatureToInternalEnergy(resourceType, MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Temperature of resource to insert with button", new(10, 0, 1_000)), mass);

						ResourceUnitData resource = new(resourceType, ChemicalType.MassToMoles(chemicalType, mass), internalEnergy);

						if (InputTracker.GetKeyDown(Key.U) && pipe.ResourceContainer.Volume + resource.Volume < pipe.ResourceContainer.MaxVolume)
						{
							pipe.ResourceContainer.AddResource(resource);
						}
					}
				}
				{
					if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Modify heat with buttons") && construction is OrdinaryPipe pipe)
					{
						ChemicalType chemicalType = MainGame.DebugSettings.AccessSetting<ComboDebugSetting<ChemicalType>>("Chemical type to insert with button", new(ChemicalType.ChemicalTypes.ToArray(), ChemicalType.Water));
						ChemicalPhase chemicalPhase = MainGame.DebugSettings.AccessSetting<EnumDebugSetting<ChemicalPhase>>("Chemical phase to insert with button", ChemicalPhase.Liquid);
						DecimalNumber mass = MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Mass to insert with button", new(10, 0, 1_000));
						ResourceType resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);

						DecimalNumber energy = MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Internal energy to modify per unit", new(1_000, 0, 10_000_000));
						DecimalNumber timeAdjustedEnergy = energy * (DecimalNumber)deltaTime;

						/*if (InputTracker.GetKeyDown(Key.H))
						{
							foreach (var unitData in pipe.ResourceContainer.EnumerateResources())
							{
								pipe.ResourceContainer.AddResource(new(unitData.ResourceType, 0, energy));
							}
						}
						else */if (InputTracker.GetKey(Key.H))
						{
							var unitData = pipe.ResourceContainer.GetResourceUnitData(resourceType);
							//foreach (var unitData in pipe.ResourceContainer.EnumerateResources()) // todo: distribute added energy evenly among resources according to mass, right now more types means greater total energy added and instant vaporization of small amounts etc. Maybe the adding should be based on mass? like add x j per kg or even per mol.
							{
								pipe.ResourceContainer.AddResource(new(unitData.ResourceType, 0, timeAdjustedEnergy));
							}
						}
						/*else if (InputTracker.GetKeyDown(Key.J))
						{
							foreach (var unitData in pipe.ResourceContainer.EnumerateResources())
							{
								pipe.ResourceContainer.AddResource(ResourceUnitData.CreateNegativeResourceUnit(unitData.ResourceType, 0, -energy));
							}
						}*/
						else if (InputTracker.GetKey(Key.J))
						{
							var unitData = pipe.ResourceContainer.GetResourceUnitData(resourceType);
							//foreach (var unitData in pipe.ResourceContainer.EnumerateResources())
							{
								pipe.ResourceContainer.AddResource(ResourceUnitData.CreateNegativeResourceUnit(unitData.ResourceType, 0, /*Make sure to not remove more energy than there is available.*/DecimalNumber.Max(-timeAdjustedEnergy, -unitData.InternalEnergy)));
							}
						}
					}
				}
				if (InputTracker.GetMouseButton(MouseButton.Right))
				{
					gameData.GameWorld.Deconstruct(construction);
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
			return lookedAtPhysicsObject is not null && (((lookedAtPhysicsObject.Entity is Connector connector && (connector).Vacant) || (lookedAtPhysicsObject.Entity is InformationProxy informationProxy && informationProxy.ProxiedEntity is Connector proxiedConnector && proxiedConnector.Vacant))) && gameData.UI.SelectedPipeType is not null;
		}

		public bool Destroyed = false;

		public void Destroy()
		{
			if (Destroyed)
				return;

			constructionMarker.Destroy();
		}

		public void Serialize(XmlWriter writer)
		{
			writer.WriteStartElement("Player");
			{
				writer.Serialize(Transform);

				writer.Serialize(LookPitch, nameof(LookPitch));
			}
			writer.WriteEndElement();
		}

		public static Player Deserialize(XmlReader reader, SerializationData serializationData)
		{
			Player player = Create(serializationData.GameData);

			reader.ReadStartElement("Player");
			{
				player.Transform = reader.DeserializeTransform();

				player.LookPitch = reader.DeserializeFixedDecimalInt4(nameof(LookPitch));
			}
			reader.ReadEndElement();

			return player;
		}
	}
}
