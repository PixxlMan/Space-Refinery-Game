﻿using FixedPrecision;
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
		public enum PerformanceStatisticsCollectorMode
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
					PhysicsTime += ((DecimalNumber)deltaTime - PhysicsTime) * (DecimalNumber)0.03;
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
					UpdateTime += ((DecimalNumber)deltaTime - UpdateTime) * (DecimalNumber)0.03;
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
					TickTime += ((DecimalNumber)deltaTime - TickTime) * (DecimalNumber)0.03;
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
					RendererFrameTime += ((DecimalNumber)deltaTime - RendererFrameTime) * (DecimalNumber)0.03;
					break;
			}
		}

		public DecimalNumber RendererFrameTime { get; private set; }

		public DecimalNumber RendererFramerate => 1 / RendererFrameTime;


		public DecimalNumber UpdateTime { get; private set; }

		public DecimalNumber UpdatesPerSecond => DecimalNumber.One / UpdateTime;

		public DecimalNumber UpdateTimeBudget => Time.UpdateInterval;

		public DecimalNumber UpdateBudgetUse => UpdateTime / UpdateTimeBudget;


		public DecimalNumber TickTime { get; private set; }

		public DecimalNumber TicksPerSecond => DecimalNumber.One / TickTime;

		public DecimalNumber TickTimeBudget => Time.TickInterval;

		public DecimalNumber TickBudgetUse => TickTime / TickTimeBudget;


		public DecimalNumber PhysicsTime { get; private set; }

		public DecimalNumber PhysicsUpdatesPerSecond => DecimalNumber.One / PhysicsTime;

		public DecimalNumber PhysicsTimeBudget => Time.PhysicsInterval;

		public DecimalNumber PhysicsBudgetUse => PhysicsTime / PhysicsTimeBudget;
	}
}