using System.Diagnostics;
using Veldrid;

namespace Space_Refinery_Engine;

public sealed class InputUpdate
{
	public readonly object UpdateSyncObject = new();

	public event Action<IntervalUnit>? CollectUpdatePerformanceData;

	public event Action<IntervalUnit>? OnUpdate;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	private GameData gameData;

	public InputUpdate(GameData gameData)
	{
		this.gameData = gameData;
	}

	public void StartUpdating()
	{
		Thread thread = new Thread(new ThreadStart(() =>
		{
			Stopwatch stopwatch = new();

			stopwatch.Start();

			TimeUnit timeLastUpdate = stopwatch.Elapsed.TotalSeconds;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (gameData.GraphicsWorld.Window.Exists)
			{
				time = stopwatch.Elapsed.TotalSeconds;

				deltaTime = time - timeLastUpdate;

				CollectUpdatePerformanceData?.Invoke(deltaTime);

				Update((IntervalUnit)DecimalNumber.Max((DN)deltaTime, (DN)Time.UpdateInterval));

				lock (responseSpinner)
					responseSpinner = Time.ResponseSpinner(time);

				Time.WaitIntervalLimit(Time.UpdateInterval, time, stopwatch, out var timeOfContinuation);

				timeLastUpdate = timeOfContinuation;
			}

			// If the Window no longer exists, close the game.
			Environment.Exit(69);
		}))
		{ Name = "Update Thread" };

		thread.Start();
	}

	private void Update(IntervalUnit deltaTime)
	{
		lock (UpdateSyncObject)
		{
			gameData.UI.Update();

			if (InputTracker.GetKeyDown(Key.P))
			{
				GameData.DebugRender.ShouldRender = !GameData.DebugRender.ShouldRender;
			}

			if (gameData.UI.InMenu || gameData.Paused)
			{
				gameData.GraphicsWorld.Window.CaptureMouse = false;
			}
			else
			{
				gameData.GraphicsWorld.Window.CaptureMouse = true;
			}

			OnUpdate?.Invoke(deltaTime);

			InputTracker.UpdateInputFrame();
		}
	}
}
