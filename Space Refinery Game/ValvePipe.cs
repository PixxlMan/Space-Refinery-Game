﻿using FixedPrecision;
using FXRenderer;
using ImGuiNET;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	public sealed class ValvePipe : Pipe
	{
		private ValvePipe()
		{
			informationProvider = new ValvePipeInformationProvider(this);
		}

		private static Mesh InternalBlockerModel;

		public FixedDecimalLong8 Limiter = (FixedDecimalLong8)0.5;

		public override void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, Connector sourceConnector)
		{
			source.TransferResource(ResourceContainers[sourceConnector], volume);
		}

		private EntityRenderable InternalBlockerRenderable;

		public Dictionary<Connector, ResourceContainer> ResourceContainers = new();

		protected override void SetUp()
		{
			if (InternalBlockerModel is null)
			{
				InternalBlockerModel = Mesh.LoadMesh(GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory, Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe", "Special", "PipeSpecialValveInternalBlocker.obj"));
			}

			InternalBlockerRenderable = EntityRenderable.Create(GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory, Transform, InternalBlockerModel, Utils.GetSolidColoredTexture(RgbaByte.LightGrey, GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory), GraphicsWorld.CameraProjViewBuffer, GraphicsWorld.LightInfoBuffer);

			GraphicsWorld.AddRenderable(InternalBlockerRenderable);

			foreach (var connector in Connectors)
			{
				ResourceContainers.Add(connector, new(PipeType.PipeProperties.FlowableVolume / Connectors.Length));
			}
		}

		protected override void Tick()
		{
			InternalBlockerRenderable.Rotation = QuaternionFixedDecimalInt4.Normalize(QuaternionFixedDecimalInt4.Concatenate(Transform.Rotation, QuaternionFixedDecimalInt4.CreateFromAxisAngle(((ITransformable)Transform).LocalUnitZ, (FixedDecimalInt4)Limiter * 90 * FixedDecimalInt4.DegreesToRadians)));

			ResourceContainer lowestFullnessContainer = ResourceContainers.Values.First();

			foreach (var resourceContainer in ResourceContainers.Values)
			{
				if (resourceContainer.Fullness < lowestFullnessContainer.Fullness)
				{
					lowestFullnessContainer = resourceContainer;
				}
			}

			foreach (var resourceContainer in ResourceContainers.Values)
			{
				if (resourceContainer == lowestFullnessContainer)
				{
					continue;
				}

				resourceContainer.TransferResource(lowestFullnessContainer, resourceContainer.Volume * Limiter * (FixedDecimalLong8)Time.TickInterval);
			}
		}

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			return ResourceContainers[pipeConnector];
		}

		protected override void Interacted()
		{
			UI.EnterMenu(DoMenu, "Valve controls");
		}

		protected override void DisplaceContents()
		{
			foreach (var connectorResourceContainerPair in ResourceContainers)
			{
				((PipeConnector)connectorResourceContainerPair.Key).TransferResource(this, connectorResourceContainerPair.Value, connectorResourceContainerPair.Value.Volume);
			}
		}

		public override void Deconstruct()
		{
			base.Deconstruct();

			InternalBlockerRenderable.Destroy();

			GraphicsWorld.UnorderedRenderables.Remove(InternalBlockerRenderable);
		}

		private float menuLimit = 0;
		private void DoMenu()
		{
			menuLimit = Limiter.ToFloat();

			ImGui.SliderFloat("Limit", ref menuLimit, 0, 1);

			Limiter = FixedDecimalLong8.FromDouble(menuLimit);
		}
	}
}