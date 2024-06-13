using FixedPrecision;
using Space_Refinery_Game_Renderer;
using ImGuiNET;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class GameWorld : IEntitySerializable
{
	public object TickSyncObject = new();

	private HashSet<Entity> entities = new();

	public event Action<IntervalUnit>? CollectTickPerformanceData;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	public void AddEntity(Entity entity)
	{
		lock (TickSyncObject)
		{
			entities.AddUnique(entity, $"This {nameof(Entity)} has already been added.");
		}
	}

	public void RemoveEntity(Entity entity)
	{
		lock (TickSyncObject)
		{
			if (!entities.Contains(entity))
			{
				return; // If the entity is not in the entities dictionary, there's nothing to remove.
			}

			entities.Remove(entity);

			entity.Destroy();
		}
	}

	public void StartTicking(MainGame mainGame) // TODO: add new game status type from which pause information could be derived instead of this?
	{
		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();

			TimeUnit timeLastUpdate = stopwatch.Elapsed;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (true)
			{
				if (!mainGame.Paused)
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
			foreach (var entity in entities)
			{
				entity.Tick();
			}
		}
	}

	public void Destroy()
	{
		lock (TickSyncObject)
		{
			Parallel.ForEach(entities, (e) =>
			{
				e.Destroy();
			});

			entities.Clear();
		}
	}

	public void DoDebugUI()
	{
		lock (TickSyncObject)
		{
			if (ImGui.Begin("Game World Debug Info"))
			{
				ImGui.Text($"Total entities: {entities.Count}");

				ImGui.End();
			}
		}
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.Serialize(entities, "Entities");
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		ConcurrentBag<Entity> entities = new();

		reader.DeserializeReferenceCollection(entities, referenceHandler, "Entities");
	}
}
