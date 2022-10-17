using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class PerformanceStatisticsCollector
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

			if (gameData.GraphicsWorld is not null)
				gameData.GraphicsWorld.CollectRenderingPerformanceData += GraphicsWorld_CollectPerformanceData;			

			if (gameData.GameWorld is not null)
				gameData.GameWorld.CollectTickPerformanceData += GameWorld_CollectPerformanceData;

			if (gameData.MainGame is not null)
				gameData.MainGame.CollectUpdatePerformanceData += MainGame_CollectPerformanceData;

			if (gameData.PhysicsWorld is not null)
				gameData.PhysicsWorld.CollectPhysicsPerformanceData += PhysicsWorld_CollectPerformanceData;
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
