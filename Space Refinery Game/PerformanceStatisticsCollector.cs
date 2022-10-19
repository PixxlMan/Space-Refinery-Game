using FixedPrecision;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class PerformanceStatisticsCollector
	{
		public enum PerformanceStatisticsCollectorMode // https://answers.unity.com/questions/326621/how-to-calculate-an-average-fps.html
		{
			Direct,
			Averaged,
		}

		GameData gameData;

		private GameWorld gameWorld;

		private GraphicsWorld graphicsWorld;

		private PhysicsWorld physicsWorld;

		private MainGame mainGame;

		public PerformanceStatisticsCollectorMode Mode;

		public PerformanceStatisticsCollector(GameData gameData, PerformanceStatisticsCollectorMode mode)
		{
			this.gameData = gameData;

			Mode = mode;

			gameData.GameDataChangedEvent += GameDataChanged;

			GameDataChanged(GameData.GameDataChange.PhysicsWorld);
			GameDataChanged(GameData.GameDataChange.GraphicsWorld);
			GameDataChanged(GameData.GameDataChange.GameWorld);
			GameDataChanged(GameData.GameDataChange.MainGame);
		}

		public void GameDataChanged(GameData.GameDataChange gameDataChange)
		{
			switch (gameDataChange)
			{
				case GameData.GameDataChange.PhysicsWorld:
					if (physicsWorld is not null)
						physicsWorld.CollectPhysicsPerformanceData -= PhysicsWorld_CollectPerformanceData;

					if (gameData.PhysicsWorld is not null)
					{
						physicsWorld = gameData.PhysicsWorld;
						physicsWorld.CollectPhysicsPerformanceData += PhysicsWorld_CollectPerformanceData;
					}
					break;
				case GameData.GameDataChange.GraphicsWorld:
					if (graphicsWorld is not null)
						graphicsWorld.CollectRenderingPerformanceData -= GraphicsWorld_CollectPerformanceData;

					if (gameData.GraphicsWorld is not null)
					{
						graphicsWorld = gameData.GraphicsWorld;
						graphicsWorld.CollectRenderingPerformanceData += GraphicsWorld_CollectPerformanceData;
					}
					break;
				case GameData.GameDataChange.GameWorld:
					if (gameWorld is not null)
						gameWorld.CollectTickPerformanceData -= GameWorld_CollectPerformanceData;

					if (gameData.GameWorld is not null)
					{
						gameWorld = gameData.GameWorld;
						gameWorld.CollectTickPerformanceData += GameWorld_CollectPerformanceData;
					}
					break;
				case GameData.GameDataChange.MainGame:
					if (mainGame is not null)
						mainGame.CollectUpdatePerformanceData -= MainGame_CollectPerformanceData;

					if (gameData.MainGame is not null)
					{
						mainGame = gameData.MainGame;
						mainGame.CollectUpdatePerformanceData += MainGame_CollectPerformanceData;
					}
					break;
			}
		}

		private void PhysicsWorld_CollectPerformanceData(FixedDecimalLong8 deltaTime)
		{
			switch (Mode)
			{
				case PerformanceStatisticsCollectorMode.Direct:
					PhysicsTime = deltaTime;
					break;
				case PerformanceStatisticsCollectorMode.Averaged:
					PhysicsTime += ((DecimalNumber)deltaTime - PhysicsTime) * (DecimalNumber)0.1;
					break;
			}
		}

		private void MainGame_CollectPerformanceData(FixedDecimalLong8 deltaTime)
		{
			switch (Mode)
			{
				case PerformanceStatisticsCollectorMode.Direct:
					UpdateTime = deltaTime;
					break;
				case PerformanceStatisticsCollectorMode.Averaged:
					UpdateTime += ((DecimalNumber)deltaTime - UpdateTime) * (DecimalNumber)0.1;
					break;
			}
		}

		private void GameWorld_CollectPerformanceData(FixedDecimalLong8 deltaTime)
		{
			switch (Mode)
			{
				case PerformanceStatisticsCollectorMode.Direct:
					TickTime = deltaTime;
					break;
				case PerformanceStatisticsCollectorMode.Averaged:
					TickTime += ((DecimalNumber)deltaTime - TickTime) * (DecimalNumber)0.1;
					break;
			}
		}

		private void GraphicsWorld_CollectPerformanceData(FixedDecimalLong8 deltaTime)
		{
			switch (Mode)
			{
				case PerformanceStatisticsCollectorMode.Direct:
					RendererFrameTime = deltaTime;
					break;
				case PerformanceStatisticsCollectorMode.Averaged:
					RendererFrameTime += ((DecimalNumber)deltaTime - RendererFrameTime) * (DecimalNumber)0.1;
					break;
			}
		}

		public DecimalNumber RendererFrameTime { get; private set; }

		public DecimalNumber RendererFramerate => 1 / (gameData.GraphicsWorld.ShouldLimitFramerate ? DecimalNumber.Max(RendererFrameTime, gameData.GraphicsWorld.FrametimeLowerLimit) : RendererFrameTime);


		public DecimalNumber UpdateTime { get; private set; }

		public DecimalNumber UpdateTimeTotal => DecimalNumber.Max(UpdateTime, UpdateTimeBudget);

		public DecimalNumber UpdatesPerSecond => DecimalNumber.One / UpdateTimeTotal;

		public DecimalNumber UpdateTimeBudget => Time.UpdateInterval;

		public DecimalNumber UpdateBudgetUse => UpdateTime / UpdateTimeBudget;


		public DecimalNumber TickTime { get; private set; }

		public DecimalNumber TickTimeTotal => DecimalNumber.Max(TickTime, TickTimeBudget);

		public DecimalNumber TicksPerSecond => DecimalNumber.One / TickTimeTotal;

		public DecimalNumber TickTimeBudget => Time.TickInterval;

		public DecimalNumber TickBudgetUse => TickTime / TickTimeBudget;


		public DecimalNumber PhysicsTime { get; private set; }

		public DecimalNumber PhysicsTimeTotal => DecimalNumber.Max(PhysicsTime, PhysicsTimeBudget);

		public DecimalNumber PhysicsUpdatesPerSecond => DecimalNumber.One / PhysicsTimeTotal;

		public DecimalNumber PhysicsTimeBudget => Time.PhysicsInterval;

		public DecimalNumber PhysicsBudgetUse => PhysicsTime / PhysicsTimeBudget;
	}
}
