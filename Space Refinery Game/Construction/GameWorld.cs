using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using Space_Refinery_Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class GameWorld
	{
		public GameWorld(GameData gameData)
		{
			GameData = gameData;
		}

		public object SynchronizationObject = new();

		public object TickSyncObject = new();

		public ConcurrentDictionary<IConstruction, EmptyType> Constructions = new();

		public GameData GameData;

		public ConcurrentDictionary<Entity, EmptyType> Entities = new();

		public event Action<FixedDecimalLong8> CollectTickPerformanceData;

		private string responseSpinner = "_";
		public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

		public void AddEntity(Entity entity)
		{
			Entities.AddUnique(entity, $"This {nameof(Entity)} has already been added.");
		}

		public void RemoveEntity(Entity entity)
		{
			//Entities.RemoveStrict(entity, $"This {nameof(Entity)} cannot be found.");
			Entities.Remove(entity, out _);

			entity.Destroy();
		}

		public void AddConstruction(IConstruction construction)
		{
			lock (SynchronizationObject)
				Constructions.AddUnique(construction);
		}

		public void Deconstruct(IConstruction construction)
		{
			Constructions.RemoveStrict(construction);

			construction.Deconstruct();

			RemoveEntity(construction);
		}

		public static Transform GenerateTransformForConnector(PositionAndDirection chosenConnectorTransform, Connector connector, FixedDecimalLong8 rotation)
		{
			QuaternionFixedDecimalInt4 connectorRotation = /*connector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(connector.Transform.Rotation) :*/ connector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			Transform pipeConnectorTransform = new Transform(connector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Direction : chosenConnectorTransform.Direction;

			Vector3FixedDecimalInt4 position = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Position : chosenConnectorTransform.Position;

			Transform transform =
				new(
					connector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransform.LocalUnitZ : pipeConnectorTransform.LocalUnitZ, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransform.LocalUnitY : pipeConnectorTransform.LocalUnitY))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransform.LocalUnitZ, -pipeConnectorTransform.LocalUnitY), QuaternionFixedDecimalInt4.CreateFromAxisAngle(direction, (FixedDecimalInt4)rotation)))
				);

			transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

			return transform;
		}

		public void StartTicking()
		{
			Thread thread = new Thread(new ThreadStart(() =>
			{
				Stopwatch stopwatch = new();
				stopwatch.Start();

				FixedDecimalLong8 timeLastUpdate = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>();
				FixedDecimalLong8 time;
				FixedDecimalLong8 deltaTime;
				while (/*GameData.MainGame.Running*/true)
				{
					if (!GameData.MainGame.Paused)
					{
						time = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>();

						deltaTime = time - timeLastUpdate;

						CollectTickPerformanceData?.Invoke(deltaTime);

						Interlocked.Increment(ref Time.TicksElapsed);

						Tick();

						lock (responseSpinner)
							responseSpinner = Time.ResponseSpinner(time);

						Time.WaitIntervalLimit(Time.TickInterval, time, stopwatch, out var timeOfContinuation);

						timeLastUpdate = timeOfContinuation;
					}
				}
			}))
			{ Name = "Tick Update Thread" };

			thread.Start();
		}

		private void Tick()
		{
			lock (TickSyncObject)
			{
				foreach (var entity in Entities.Keys)
				{
					entity.Tick();
				}
			}
		}

		public void ClearAll()
		{
			lock (TickSyncObject)
			{
				Parallel.ForEach(Entities.Keys, RemoveEntity);
			}
		}
	}
}
