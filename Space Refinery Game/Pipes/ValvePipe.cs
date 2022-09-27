using FixedPrecision;
using FXRenderer;
using ImGuiNET;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

		public override void TransferResourceFromConnector(ResourceContainer source, FixedDecimalLong8 volume, PipeConnector sourceConnector)
		{
			lock (this)
			{
				source.TransferResource(ResourceContainers[sourceConnector], volume);
			}
		}

		private EntityRenderable InternalBlockerRenderable;

		public Dictionary<PipeConnector, ResourceContainer> ResourceContainers = new();

		protected override void SetUp()
		{
			lock (this)
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
		}

		protected override void Tick()
		{
			lock (this)
			{
				InternalBlockerRenderable.Transform.Rotation = QuaternionFixedDecimalInt4.Normalize(QuaternionFixedDecimalInt4.Concatenate(Transform.Rotation, QuaternionFixedDecimalInt4.CreateFromAxisAngle(Transform.LocalUnitZ, (FixedDecimalInt4)Limiter * 90 * FixedDecimalInt4.DegreesToRadians)));

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
		}

		public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
		{
			lock (this)
			{
				return ResourceContainers[pipeConnector];
			}
		}

		protected override void Interacted()
		{
			UI.EnterMenu(DoMenu, "Valve controls");
		}

		protected override void DisplaceContents()
		{
			lock (this)
			{
				foreach (var connectorResourceContainerPair in ResourceContainers)
				{
					if (connectorResourceContainerPair.Key.Vacant)
						continue;

					(connectorResourceContainerPair.Key).TransferResource(this, connectorResourceContainerPair.Value, connectorResourceContainerPair.Value.Volume);
				}
			}
		}

		public override void Deconstruct()
		{
			lock (this)
			{
				base.Deconstruct();

				InternalBlockerRenderable.Destroy();

				GraphicsWorld.UnorderedRenderables.Remove(InternalBlockerRenderable);
			}
		}

		private float menuLimit = 0;
		private void DoMenu()
		{
			menuLimit = Limiter.ToFloat();

			ImGui.SliderFloat("Limit", ref menuLimit, 0, 1);

			Limiter = FixedDecimalLong8.FromDouble(menuLimit);
		}


		protected override void SerializeState(XmlWriter writer)
		{
			/*Limiter.Serialize(writer, "Limiter");

			writer.Serialize(ResourceContainers, (w, c) => c.Value.Serialize(w));*/
		}

		protected override void DeserializeState(XmlReader reader)
		{
			/*Limiter = reader.DeserializeFixedDecimalLong8("Limiter");

			reader.DeserializeCollection((r) => );*/
		}
	}
}
