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

		public DecimalNumber Limiter = (DecimalNumber)0.5;

		public override void TransferResourceFromConnector(ResourceContainer source, DecimalNumber volume, PipeConnector sourceConnector)
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
					InternalBlockerModel = GraphicsWorld.MeshLoader.LoadCached(Path.Combine(Environment.CurrentDirectory, "Assets", "Models", "Pipe", "Special", "PipeSpecialValveInternalBlocker.obj"));
				}

				InternalBlockerRenderable = EntityRenderable.Create(GraphicsWorld, Transform, InternalBlockerModel, Utils.GetSolidColoredTexture(RgbaByte.LightGrey, GraphicsWorld.GraphicsDevice, GraphicsWorld.Factory), GraphicsWorld.CameraProjViewBuffer, GraphicsWorld.LightInfoBuffer);

				foreach (var connector in Connectors)
				{
					ResourceContainers.Add(connector, new(PipeType.PipeProperties.FlowableVolume / Connectors.Length));
				}
			}
		}

		public override void Tick()
		{
			lock (this)
			{
				base.Tick();

				InternalBlockerRenderable.Transform.Rotation = QuaternionFixedDecimalInt4.Normalize(QuaternionFixedDecimalInt4.Concatenate(Transform.Rotation, QuaternionFixedDecimalInt4.CreateFromAxisAngle(Transform.LocalUnitZ, (DecimalNumber)Limiter * 90 * DecimalNumber.DegreesToRadians)));

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

					resourceContainer.TransferResource(lowestFullnessContainer, resourceContainer.Volume * Limiter * (DecimalNumber)Time.TickInterval);
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
			}
		}

		private float menuLimit = 0;
		private void DoMenu()
		{
			menuLimit = Limiter.ToFloat();

			ImGui.SliderFloat("Limit", ref menuLimit, 0, 1);

			Limiter = DecimalNumber.FromDouble(menuLimit);
		}


		public override void SerializeState(XmlWriter writer)
		{
			base.SerializeState(writer);

			writer.Serialize(Limiter, nameof(Limiter));

			writer.Serialize(ResourceContainers, (w, c) => c.Value.Serialize(w));
		}

		public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			base.DeserializeState(reader, serializationData, referenceHandler);

			Limiter = reader.DeserializeDecimalNumber(nameof(Limiter));
			var resourceContainers = (ResourceContainer[])reader.DeserializeCollection((r) => ResourceContainer.Deserialize(r));

			serializationData.SerializationCompleteEvent += () =>
			{
				for (int i = 0; i < resourceContainers.Length; i++)
				{
					serializationData.SerializationCompleteEvent += () => ResourceContainers.Add(Connectors[i], resourceContainers[i]);
				}
			};
		}
	}
}
