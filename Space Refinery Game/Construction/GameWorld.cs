using FixedPrecision;
using FXRenderer;
using ImGuiNET;
using Space_Refinery_Utilities;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Space_Refinery_Game;

public sealed class GameWorld
{
	public GameWorld(GameData gameData)
	{
		GameData = gameData;
	}

	public object TickSyncObject = new();

	public GameData GameData;

	public ConcurrentDictionary<IConstruction, EmptyType> Constructions = new();

	public ConcurrentDictionary<Entity, EmptyType> Entities = new();

	public event Action<IntervalUnit> CollectTickPerformanceData;

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
		lock (TickSyncObject)
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

			TimeUnit timeLastUpdate = stopwatch.Elapsed;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (/*GameData.MainGame.Running*/true)
			{
				if (!GameData.MainGame.Paused)
				{
					time = stopwatch.Elapsed;

					deltaTime = time - timeLastUpdate;

					CollectTickPerformanceData?.Invoke(deltaTime);

					Interlocked.Increment(ref Time.TicksElapsed);

					Tick();

					Time.ResponseSpinner(time, ref responseSpinner);

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
			foreach (Entity entity in Entities.Keys)
			{
				RemoveEntity(entity);
			}
		}
	}

	public void ClearAllParallell()
	{
		lock (TickSyncObject)
		{
			Parallel.ForEach(Entities.Keys, RemoveEntity);
		}
	}

	/// <remarks>
	/// This method simply removes the references to all entites, it does not call Destroy methods. All entities must be responsibly Destroyed manually - don't litter!
	/// </remarks>
	public void ResetUnsafe()
	{
		lock (TickSyncObject)
		{
			Entities.Clear();
			Constructions.Clear();
		}
	}

	public void DoDebugUI()
	{
		if (ImGui.Begin("Game World Debug Info"))
		{
			ImGui.Text($"Total entities: {Entities.Count}");

			ImGui.Text($"Total constructions: {Constructions.Count}");

			ImGui.End();
		}
	}
}
