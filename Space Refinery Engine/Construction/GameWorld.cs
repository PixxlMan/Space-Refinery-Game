using FixedPrecision;
using FXRenderer;
using ImGuiNET;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class GameWorld : IEntitySerializable
{
	public object TickSyncObject = new();

	private HashSet<IConstruction> constructions = new();

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
				return; // If the construction was not in the constructions dictionary there's nothing to deconstruct.
			}

			entities.Remove(entity);

			entity.Destroy();
		}
	}

	public void AddConstruction(IConstruction construction)
	{
		lock (TickSyncObject)
		{
			AddEntity(construction);

			constructions.AddUnique(construction);
		}
	}

	public void Deconstruct(IConstruction construction)
	{
		lock (TickSyncObject)
		{
			if (!constructions.Contains(construction))
			{
				return; // If the construction was not in the constructions dictionary there's nothing to deconstruct.
			}

			construction.Deconstruct();

			RemoveEntity(construction);
		}
	}

	public void StartTicking(MainGame mainGame)
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
			constructions.Clear();
		}
	}

	public void DoDebugUI()
	{
		lock (TickSyncObject)
		{
			if (ImGui.Begin("Game World Debug Info"))
			{
				ImGui.Text($"Total entities: {entities.Count}");

				ImGui.Text($"Total constructions: {constructions.Count}");

				ImGui.End();
			}
		}
	}

	public void SerializeState(XmlWriter writer, SerializationData serializationData)
	{
		writer.Serialize(entities, nameof(entities));

		writer.Serialize(constructions, nameof(constructions));
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		ConcurrentBag<Entity> entities = new();

		reader.DeserializeReferenceCollection(entities, referenceHandler, nameof(entities));

		ConcurrentBag<IConstruction> constructions = new();

		reader.DeserializeReferenceCollection(constructions, referenceHandler, nameof(entities));
	}
}
