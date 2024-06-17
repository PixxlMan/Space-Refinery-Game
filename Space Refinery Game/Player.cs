using FixedPrecision;
using Space_Refinery_Engine;
using Space_Refinery_Game_Renderer;
using System.Xml;
using Veldrid;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Game;

public sealed class Player : ISerializableReference
{
	public Transform Transform = Transform.Identity;

	private GameData gameData;

	private ConstructionMarker constructionMarker;

	public FixedDecimalInt4 LookPitch;

	public Transform CameraTransform =>
		new(
			Transform.Position,
			QuaternionFixedDecimalInt4.Concatenate(
				QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(
					FixedDecimalInt4.Zero,
					LookPitch,
					FixedDecimalInt4.Zero),
				Transform.Rotation
				).NormalizeQuaternion()
			);

	private SerializableReference serializableReference = Guid.NewGuid();
	public SerializableReference SerializableReference => serializableReference;

	private PhysicsObject? lookedAtPhysicsObject;

	private Player()
	{

	}

	public void SetUp(GameData gameData)
	{
		this.gameData = gameData;
		constructionMarker = ConstructionMarker.Create(gameData.GraphicsWorld);
	}

	public static Player Create(GameData gameData)
	{
		Player player = new();

		player.SetUp(gameData);

		gameData.Game.GameReferenceHandler.RegisterReference(player);

		gameData.InputUpdate.OnUpdate += player.Update;

		return player;
	}

	public void Update(IntervalUnit deltaTime)
	{
		if (gameData.Paused || gameData.UI.InMenu)
			return;

		gameData.GraphicsWorld.Camera.Transform = CameraTransform;

		lookedAtPhysicsObject = gameData.PhysicsWorld.Raycast(CameraTransform.Position, -CameraTransform.LocalUnitZ, 1000);

		if (lookedAtPhysicsObject is not null)
		{
			gameData.UI.SetTargetedPhysicsObject(lookedAtPhysicsObject);
			gameData.UI.SetSelectedInformationProvider(lookedAtPhysicsObject.InformationProvider);
		}
		else
		{
			gameData.UI.ClearTargetedPhysicsObject();
			gameData.UI.ClearSelectedInformationProvider();
		}

		if (lookedAtPhysicsObject is not null)
		{
			if (InputTracker.GetKeyDown(Key.F))
			{
				lookedAtPhysicsObject.Entity.Interacted();
			}
		}

		if (ShouldShowConstructionMarker(lookedAtPhysicsObject)) // An entity with no connectors will never return true and we can therefore safely assume there will always be a connector selection.
		{
			PipeConnector pipeConnector = (PipeConnector)lookedAtPhysicsObject!.Entity;

			var selectedEntityType = (PipeType)gameData.UI.SelectedEntityType;

			constructionMarker.SetMesh(selectedEntityType!.Mesh);

			constructionMarker.SetTransform(Connector.GenerateTransformForConnector(selectedEntityType.ConnectorPlacements[gameData.UI.ConnectorSelection!.Value], pipeConnector, gameData.UI.RotationSnapped));

			constructionMarker.ShouldDraw = true;

			if (Pipe.ValidateBuild(pipeConnector, selectedEntityType, gameData.UI.ConnectorSelection!.Value, gameData.UI.RotationSnapped, gameData))
			{
				constructionMarker.State = ConstructionMarker.ConstructionMarkerState.LegalBuild;

				if (InputTracker.GetMouseButton(MouseButton.Left))
				{
					Pipe.Build(pipeConnector, selectedEntityType, gameData.UI.ConnectorSelection!.Value, gameData.UI.RotationSnapped, gameData, gameData.Game.GameReferenceHandler);

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
			if (InputTracker.GetKeyDown(Key.F10))
			{
				DebugStopPoints.RegisterStopPoint(construction.SerializableReference);
			}

			if (construction is OrdinaryPipe pipe)
			{
				if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Insert resource with button"))
				{
					ChemicalType chemicalType = GameData.DebugSettings.AccessSetting<ComboDebugSetting<ChemicalType>>("Chemical type to insert with button", new(ChemicalType.ChemicalTypes.ToArray(), ChemicalType.Water));

					TemperatureUnit temperatureUnit = (TemperatureUnit)(DecimalNumber)GameData.DebugSettings.AccessSetting<SliderDebugSetting>("Temperature of resource to insert with button", new(10, 0, 1_000));

					ChemicalPhase chemicalPhase = chemicalType.GetChemicalPhaseForTemperature(temperatureUnit);

					MassUnit mass = (MassUnit)(DecimalNumber)GameData.DebugSettings.AccessSetting<SliderDebugSetting>("Mass to insert with button", new(10, 0, 1_000));

					ResourceType resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);

					EnergyUnit internalEnergy = ChemicalType.TemperatureToInternalEnergy(resourceType, temperatureUnit, mass);

					ResourceUnitData resource = new(resourceType, ChemicalType.MassToMoles(chemicalType, mass), internalEnergy);

					if (InputTracker.GetKeyDown(Key.U) && pipe.ResourceContainer.NonCompressableVolume + resource.Volume < pipe.ResourceContainer.VolumeCapacity)
					{
						pipe.ResourceContainer.AddResource(resource);
					}
				}

				if (GameData.DebugSettings.AccessSetting<BooleanDebugSetting>("Modify heat with buttons"))
				{
					ChemicalType chemicalType = GameData.DebugSettings.AccessSetting<ComboDebugSetting<ChemicalType>>("Chemical type to modify heat of with button", new(ChemicalType.ChemicalTypes.ToArray(), ChemicalType.Water));
					ChemicalPhase chemicalPhase = GameData.DebugSettings.AccessSetting<EnumDebugSetting<ChemicalPhase>>("Chemical phase to modify heat of with button", ChemicalPhase.Liquid);
					ResourceType resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);

					EnergyUnit energy = (EnergyUnit)(DN)GameData.DebugSettings.AccessSetting<SliderDebugSetting>("Internal energy to modify per unit", new(1_000, 0, 10_000_000));
					EnergyUnit timeAdjustedEnergy = energy * deltaTime;

					/*if (InputTracker.GetKeyDown(Key.H))
					{
						foreach (var unitData in pipe.ResourceContainer.EnumerateResources())
						{
							pipe.ResourceContainer.AddResource(new(unitData.ResourceType, 0, energy));
						}
					}
					else */
					if (InputTracker.GetKey(Key.H))
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
							pipe.ResourceContainer.AddResource(ResourceUnitData.CreateNegativeResourceUnit(unitData.ResourceType, 0, /*Make sure to not remove more energy than there is available.*/UnitsMath.Max(-timeAdjustedEnergy, -unitData.InternalEnergy)));
						}
					}
				}
			}

			if (InputTracker.GetMouseButton(MouseButton.Right))
			{
				gameData.Game.GameWorld.RemoveEntity(construction);
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
			Transform.Position += motionDir * sprintFactor * (FixedDecimalInt4)(DN)deltaTime;
		}

		FixedDecimalInt4 yawDelta = -InputTracker.MouseDelta.X / 300;
		FixedDecimalInt4 pitchDelta = -InputTracker.MouseDelta.Y / 300;

		LookPitch += pitchDelta;

		LookPitch = FixedDecimalInt4.Clamp(LookPitch, -80 * FixedDecimalInt4.DegreesToRadians, 80 * FixedDecimalInt4.DegreesToRadians);

		Transform.Rotation = QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(yawDelta, FixedDecimalInt4.Zero, FixedDecimalInt4.Zero), Transform.Rotation).NormalizeQuaternion();
		//Transform.Rotation = QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(FixedDecimalInt4.Zero, LookPitch, FixedDecimalInt4.Zero), Transform.Rotation).NormalizeQuaternion();
	}

	private bool ShouldShowConstructionMarker(PhysicsObject? lookedAtPhysicsObject)
	{
		return lookedAtPhysicsObject is not null && (((lookedAtPhysicsObject.Entity is Connector connector && (connector).Vacant))) && gameData.UI.SelectedEntityType is PipeType;
	}

	public bool Destroyed = false;

	public void Destroy()
	{
		if (Destroyed)
			return;

		constructionMarker.Destroy();

		Destroyed = true;
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(this, "Reference");

		writer.Serialize(Transform, nameof(Transform));

		writer.Serialize(LookPitch, nameof(LookPitch));
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		serializableReference = reader.ReadReference("Reference");

		Transform = reader.DeserializeTransform(nameof(Transform));

		LookPitch = reader.DeserializeFixedDecimalInt4(nameof(LookPitch));

		SetUp(serializationData.GameData);
	}
}
