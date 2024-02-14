using CsvHelper;
using CsvHelper.Configuration.Attributes;
using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using Space_Refinery_Utilities;
using System.Globalization;
using System.Xml;
using Veldrid;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Engine
{
	public sealed class Player
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

		private PhysicsObject? lookedAtPhysicsObject;

		public PhysicsObject? LookedAtPhysicsObject { get { return lookedAtPhysicsObject; } }


		public OrdinaryPipe? RecordingTarget;

		public bool CSVRecording;

		public const int UpdatesPerCSVRecordTaken = 10; // 200 Hz / 10 = 20 Hz

		public int UpdatesCount = 0;

		public record struct PressureTempRecord(Decimal pressure, Decimal temp)
		{
			[Name("Pressure")]
			public Decimal Pressure = pressure;

			[Name("Temperature")]
			public Decimal Temperature = temp;
		}

		public List<PressureTempRecord> RecordedPressureAndTemp = new();

		public void CSVRecord()
		{
			if (RecordingTarget is null)
			{
				Logging.LogError("No recording target specified!");
			}

			if (CSVRecording)
			{
				CSVRecording = false;

				UpdatesCount = 0;

				using CsvWriter writer = new(new StreamWriter(File.OpenWrite("R:\\pressure_temperature.csv")), CultureInfo.InvariantCulture, false);

				writer.WriteRecords(RecordedPressureAndTemp);

				writer.Flush();

				writer.Dispose();

				RecordedPressureAndTemp.Clear();
			}
			else
			{
				CSVRecording = true;
			}
		}

		private void CSVRecordUpdate(OrdinaryPipe ordinaryPipe)
		{
			if (!CSVRecording)
			{
				return;
			}

			UpdatesCount++;

			if (UpdatesCount == UpdatesPerCSVRecordTaken)
			{
				UpdatesCount = 0;

				RecordedPressureAndTemp.Add(new((decimal)(DecimalNumber)ordinaryPipe.ResourceContainer.Pressure, (decimal)(DecimalNumber)ordinaryPipe.ResourceContainer.AverageTemperature));

				lock (gameData.Game.GameWorld.TickSyncObject)
				{
					ordinaryPipe.ResourceContainer.maxVolume -= 0.0001;
				}
			}
		}


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

		public void Update(IntervalUnit deltaTime)
		{
			lookedAtPhysicsObject = gameData.PhysicsWorld.Raycast(CameraTransform.Position, -CameraTransform.LocalUnitZ, 1000);

			if (lookedAtPhysicsObject is not null)
			{
				gameData.UI.CurrentlySelectedInformationProvider = lookedAtPhysicsObject.InformationProvider;
				gameData.UI.CurrentlyLookedAtPhysicsObject = lookedAtPhysicsObject;
			}
			else
			{
				gameData.UI.CurrentlySelectedInformationProvider = null;
				gameData.UI.CurrentlyLookedAtPhysicsObject = null;
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
				PipeConnector pipeConnector = (PipeConnector)lookedAtPhysicsObject.Entity;

				constructionMarker.SetMesh(gameData.UI.SelectedPipeType.Mesh);

				constructionMarker.SetTransform(Connector.GenerateTransformForConnector(gameData.UI.SelectedPipeType.ConnectorPlacements[gameData.UI.ConnectorSelection], pipeConnector, gameData.UI.RotationSnapped));

				constructionMarker.ShouldDraw = true;

				if (Pipe.ValidateBuild(pipeConnector, gameData.UI.SelectedPipeType, gameData.UI.ConnectorSelection, gameData.UI.RotationSnapped, gameData))
				{
					constructionMarker.State = ConstructionMarker.ConstructionMarkerState.LegalBuild;

					if (InputTracker.GetMouseButton(MouseButton.Left))
					{
						Pipe.Build(pipeConnector, gameData.UI.SelectedPipeType, gameData.UI.ConnectorSelection, gameData.UI.RotationSnapped, gameData, gameData.Game.GameReferenceHandler);

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
					CSVRecordUpdate(pipe);

					if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Insert 100 g air with 'I'"))
					{
						TemperatureUnit temperature = 20;

						MassUnit totalMass = 0.1;

						var nitrogen = ChemicalType.GetChemicalType("Nitrogen");
						var nitrogenMass = (78.084 / 100) * totalMass;
						MolesUnit nitrogenPart = ChemicalType.MassToMoles(nitrogen, nitrogenMass);

						var oxygen = ChemicalType.Oxygen;
						var oxygenMass = (20.948 / 100) * totalMass;
						MolesUnit oxygenPart = ChemicalType.MassToMoles(oxygen, oxygenMass);

						var water = ChemicalType.Water;
						var waterMass = (1 / 100) * totalMass;
						MolesUnit waterPart = ChemicalType.MassToMoles(water, waterMass);

						var argon = ChemicalType.GetChemicalType("Argon");
						var argonMass = (0.934 / 100) * totalMass;
						MolesUnit argonPart = ChemicalType.MassToMoles(argon, argonMass);

						if (InputTracker.GetKeyDown(Key.I))
						{
							pipe.ResourceContainer.AddResource(new(nitrogen.GasPhaseType, nitrogenPart, ChemicalType.TemperatureToInternalEnergy(nitrogen.GasPhaseType, temperature, nitrogenMass)));
							pipe.ResourceContainer.AddResource(new(oxygen.GasPhaseType, oxygenPart, ChemicalType.TemperatureToInternalEnergy(oxygen.GasPhaseType, temperature, oxygenMass)));
							pipe.ResourceContainer.AddResource(new(water.GasPhaseType, oxygenPart, ChemicalType.TemperatureToInternalEnergy(water.GasPhaseType, temperature, waterMass)));
							pipe.ResourceContainer.AddResource(new(argon.GasPhaseType, argonPart, ChemicalType.TemperatureToInternalEnergy(argon.GasPhaseType, temperature, argonMass)));
						}
					}

					if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Select pipe to record"))
					{
						if (RecordingTarget is null)
						{
							RecordingTarget = pipe;
						}
					}
					else
					{
						RecordingTarget = null;
					}

					if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Insert resource with button"))
					{
						ChemicalType chemicalType = MainGame.DebugSettings.AccessSetting<ComboDebugSetting<ChemicalType>>("Chemical type to insert with button", new(ChemicalType.ChemicalTypes.ToArray(), ChemicalType.Water));

						TemperatureUnit temperatureUnit = (TemperatureUnit)(DecimalNumber)MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Temperature of resource to insert with button", new(10, 0, 1_000));

						ChemicalPhase chemicalPhase = chemicalType.GetChemicalPhaseForTemperature(temperatureUnit);

						MassUnit mass = (MassUnit)(DecimalNumber)MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Mass to insert with button", new(10, 0, 1_000));

						ResourceType resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);

						EnergyUnit internalEnergy = ChemicalType.TemperatureToInternalEnergy(resourceType, temperatureUnit, mass);

						ResourceUnitData resource = new(resourceType, ChemicalType.MassToMoles(chemicalType, mass), internalEnergy);

						if (InputTracker.GetKeyDown(Key.U) && pipe.ResourceContainer.Volume + resource.Volume < pipe.ResourceContainer.MaxVolume)
						{
							pipe.ResourceContainer.AddResource(resource);
						}
					}

					if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Modify heat with buttons"))
					{
						ChemicalType chemicalType = MainGame.DebugSettings.AccessSetting<ComboDebugSetting<ChemicalType>>("Chemical type to modify heat of with button", new(ChemicalType.ChemicalTypes.ToArray(), ChemicalType.Water));
						ChemicalPhase chemicalPhase = MainGame.DebugSettings.AccessSetting<EnumDebugSetting<ChemicalPhase>>("Chemical phase to modify heat of with button", ChemicalPhase.Liquid);
						ResourceType resourceType = chemicalType.GetResourceTypeForPhase(chemicalPhase);

						EnergyUnit energy = (EnergyUnit)(DN)MainGame.DebugSettings.AccessSetting<SliderDebugSetting>("Internal energy to modify per unit", new(1_000, 0, 10_000_000));
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

		private bool ShouldShowConstructionMarker(PhysicsObject lookedAtPhysicsObject)
		{
			return lookedAtPhysicsObject is not null && (((lookedAtPhysicsObject.Entity is Connector connector && (connector).Vacant))) && gameData.UI.SelectedPipeType is not null;
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
