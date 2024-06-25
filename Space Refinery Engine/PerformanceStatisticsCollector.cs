using Space_Refinery_Engine.Renderer;
using Space_Refinery_Engine;
using System.Diagnostics;

namespace Space_Refinery_Engine;

public sealed class PerformanceStatisticsCollector
{
	public enum PerformanceStatisticsCollectorMode // https://answers.unity.com/questions/326621/how-to-calculate-an-average-fps.html
	{
		Direct,
		Averaged,
	}

	GameData gameData;

	public PerformanceStatisticsCollectorMode Mode;

	public PerformanceStatisticsCollector(GameData gameData, PerformanceStatisticsCollectorMode mode)
	{
		this.gameData = gameData;

		Mode = mode;

		Restore();

		GameDataChanged(GameData.GameDataChange.PhysicsWorld);
		GameDataChanged(GameData.GameDataChange.GraphicsWorld);
		GameDataChanged(GameData.GameDataChange.Game);
		GameDataChanged(GameData.GameDataChange.InputUpdate);
	}

	public void Restore()
	{
		gameData.GameDataChangedEvent += GameDataChanged;
	}

	public void GameDataChanged(GameData.GameDataChange gameDataChange)
	{
		switch (gameDataChange)
		{
			case GameData.GameDataChange.PhysicsWorld:
				if (gameData.PhysicsWorld is not null)
					gameData.PhysicsWorld.CollectPhysicsPerformanceData -= PhysicsWorld_CollectPerformanceData;

				if (gameData.PhysicsWorld is not null)
				{
					gameData.PhysicsWorld.CollectPhysicsPerformanceData += PhysicsWorld_CollectPerformanceData;
				}
				break;
			case GameData.GameDataChange.GraphicsWorld:
				if (gameData.GraphicsWorld is not null) // TODO: we need to either keep the old ones, or better yet, just replace the performance collector every time? why not.
					gameData.GraphicsWorld.CollectRenderingPerformanceData -= GraphicsWorld_CollectPerformanceData;

				if (gameData.GraphicsWorld is not null)
				{
					gameData.GraphicsWorld.CollectRenderingPerformanceData += GraphicsWorld_CollectPerformanceData;
				}
				break;
			case GameData.GameDataChange.Game:
				if (gameData.Game is not null)
					gameData.Game.GameWorld.CollectTickPerformanceData -= GameWorld_CollectPerformanceData;

				if (gameData.Game is not null)
				{
					gameData.Game.GameWorld.CollectTickPerformanceData += GameWorld_CollectPerformanceData;
				}
				break;
			case GameData.GameDataChange.InputUpdate:
				if (gameData.InputUpdate is not null)
					gameData.InputUpdate.CollectUpdatePerformanceData -= InputUpdate_CollectPerformanceData;

				if (gameData.InputUpdate is not null)
				{
					gameData.InputUpdate.CollectUpdatePerformanceData += InputUpdate_CollectPerformanceData;
				}
				break;
		}
	}

	private static Portion<TimeUnit> smoothing = (Portion<TimeUnit>)0.1;

	private static readonly IntervalUnit deltaTimeStutterWarningThreshold = (IntervalUnit)0.05;

	[Conditional("DEBUG")]
	private void DebugPerfWarn(IntervalUnit deltaTime, string system)
	{
		if (deltaTime > deltaTimeStutterWarningThreshold)
		{
			Logging.LogWarning($"{FormatUnit.FormatTime(deltaTime)} stutter in {system}");
		}
	}

	private void PhysicsWorld_CollectPerformanceData(IntervalUnit deltaTime)
	{
		DebugPerfWarn(deltaTime, nameof(PhysicsWorld));

		switch (Mode)
		{
			case PerformanceStatisticsCollectorMode.Direct:
				PhysicsTime = deltaTime;
				break;
			case PerformanceStatisticsCollectorMode.Averaged:
				PhysicsTime += (deltaTime - PhysicsTime) * smoothing;
				break;
		}
	}

	private void InputUpdate_CollectPerformanceData(IntervalUnit deltaTime)
	{
		DebugPerfWarn(deltaTime, nameof(InputUpdate));

		switch (Mode)
		{
			case PerformanceStatisticsCollectorMode.Direct:
				UpdateTime = deltaTime;
				break;
			case PerformanceStatisticsCollectorMode.Averaged:
				UpdateTime += (deltaTime - UpdateTime) * smoothing;
				break;
		}
	}

	private void GameWorld_CollectPerformanceData(IntervalUnit deltaTime)
	{
		DebugPerfWarn(deltaTime, nameof(GameWorld));

		switch (Mode)
		{
			case PerformanceStatisticsCollectorMode.Direct:
				TickTime = deltaTime;
				break;
			case PerformanceStatisticsCollectorMode.Averaged:
				TickTime += (deltaTime - TickTime) * smoothing;
				break;
		}
	}

	private void GraphicsWorld_CollectPerformanceData(IntervalUnit deltaTime)
	{
		DebugPerfWarn(deltaTime, nameof(GraphicsWorld));

		switch (Mode)
		{
			case PerformanceStatisticsCollectorMode.Direct:
				RendererFrameTime = deltaTime;
				break;
			case PerformanceStatisticsCollectorMode.Averaged:
				RendererFrameTime += (deltaTime - RendererFrameTime) * smoothing;
				break;
		}
	}

	public TimeUnit RendererFrameTime { get; private set; }

	public RateUnit RendererFramerate => IntervalRateConversionUnit.Unit / (gameData.GraphicsWorld.ShouldLimitFramerate ? (IntervalUnit)DecimalNumber.Max((DN)RendererFrameTime, (DN)gameData.GraphicsWorld.FrametimeLowerLimit) : (IntervalUnit)RendererFrameTime);


	public TimeUnit UpdateTime { get; private set; }

	public IntervalUnit UpdateTimeTotal => (IntervalUnit)DecimalNumber.Max((DN)UpdateTime, (DN)UpdateTimeBudget);

	public RateUnit UpdatesPerSecond => IntervalRateConversionUnit.Unit / UpdateTimeTotal;

	public TimeUnit UpdateTimeBudget => Time.UpdateInterval;

	public Portion<TimeUnit> UpdateBudgetUse => UpdateTime / UpdateTimeBudget;


	public TimeUnit TickTime { get; private set; }

	public IntervalUnit TickTimeTotal => (IntervalUnit)DecimalNumber.Max((DN)TickTime, (DN)TickTimeBudget);

	public RateUnit TicksPerSecond => IntervalRateConversionUnit.Unit / TickTimeTotal;

	public TimeUnit TickTimeBudget => Time.TickInterval;

	public Portion<TimeUnit> TickBudgetUse => TickTime / TickTimeBudget;


	public TimeUnit PhysicsTime { get; private set; }

	public IntervalUnit PhysicsTimeTotal => (IntervalUnit)DecimalNumber.Max((DN)PhysicsTime, (DN)PhysicsTimeBudget);

	public RateUnit PhysicsUpdatesPerSecond => IntervalRateConversionUnit.Unit / PhysicsTimeTotal;

	public TimeUnit PhysicsTimeBudget => Time.PhysicsInterval;

	public Portion<TimeUnit> PhysicsBudgetUse => PhysicsTime / PhysicsTimeBudget;
}
