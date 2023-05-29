using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Veldrid;

namespace Space_Refinery_Game
{
	public sealed class PipeType : IEntityType, ISerializableReference
	{
		public static ConcurrentDictionary<string, PipeType> PipeTypes = new();

		public PositionAndDirection[] ConnectorPlacements;

		public PipeConnectorProperties[] ConnectorProperties;

		public string[] ConnectorNames;

		public string ModelPath;

		public Mesh Mesh { get; private set; }

		public PipeProperties PipeProperties;

		public Type TypeOfPipe;

		public BatchRenderable BatchRenderable { get; private set; }

		public SerializableReference SerializableReference { get; private set; }

		private static PipeConnectorProperties standardPipeConnectorProperties;
		public static PipeConnectorProperties StandardConnectorProperties
		{
			get
			{
				if (standardPipeConnectorProperties is null)
				{
					standardPipeConnectorProperties = (PipeConnectorProperties)MainGame.GlobalReferenceHandler[Guid.Parse(/*Reference to standard pipe connector properties.*/"a2f1a2e0-529e-41e3-bb90-544104b85d2a")];
				}

				return standardPipeConnectorProperties;
			}
		}

		public string Name;

		public PipeType(string name, PositionAndDirection[] connectorPlacements, PipeConnectorProperties[] connectorProperties, string[] connectorNames, string modelPath, Mesh mesh, PipeProperties pipeProperties, Type typeOfPipe)
		{
			Name = name;
			ConnectorPlacements = connectorPlacements;
			ConnectorProperties = connectorProperties;
			ConnectorNames = connectorNames;
			ModelPath = modelPath;
			Mesh = mesh;
			PipeProperties = pipeProperties;
			TypeOfPipe = typeOfPipe;

			SerializableReference = Guid.NewGuid();

			if (!PipeTypes.TryAdd(Name, this))
			{
				throw new Exception($"Couldn't add {nameof(PipeType)} '{Name}' to dictionary of all available PipeTypes.");
			}
		}

		private PipeType()
		{

		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.WriteElementString(nameof(Name), Name);

			PipeProperties.SerializeState(writer);

			writer.Serialize(ConnectorPlacements, (w, pd) => pd.SerializeState(w), nameof(ConnectorPlacements));

			writer.Serialize(ConnectorProperties, (w, cp) => w.SerializeReference(cp), nameof(ConnectorProperties));

			writer.Serialize(ConnectorNames is not null, "HasConnectorNames");

			if (ConnectorNames is not null)
			{
				writer.Serialize(ConnectorNames, (w, s) => w.WriteElementString("ConnectorName", s));
			}

			writer.WriteElementString(nameof(ModelPath), ModelPath);

			writer.Serialize(TypeOfPipe);
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SerializableReference = reader.ReadReference();

			Name = reader.ReadString(nameof(Name));

			PipeProperties.DeserializeState(reader, serializationData, referenceHandler);

			ConnectorPlacements = (PositionAndDirection[])reader.DeserializeCollection(
				(r) =>
				{
					PositionAndDirection positionAndDirection = new();
					positionAndDirection.DeserializeState(reader, serializationData, referenceHandler);
					return positionAndDirection;
				}, nameof(ConnectorPlacements));

			List<PipeConnectorProperties> pipeConnectorProperties = new();
			reader.DeserializeCollection(
				(r) =>
				{
					r.DeserializeReference<PipeConnectorProperties>(referenceHandler, (pcp) => pipeConnectorProperties.Add(pcp));
				}, nameof(ConnectorProperties));
			serializationData.DeserializationCompleteEvent += () => ConnectorProperties = pipeConnectorProperties.ToArray();

			if (reader.DeserializeBoolean("HasConnectorNames"))
			{
				ConnectorNames = (string[])reader.DeserializeCollection((r) => r.ReadString("ConnectorName"));
			}

			ModelPath = reader.ReadString(nameof(ModelPath));

			Mesh = serializationData.GameData.GraphicsWorld.MeshLoader.LoadCached(ModelPath);

			TypeOfPipe = reader.DeserializeType();

			if (!PipeTypes.TryAdd(Name, this))
			{
				throw new Exception($"Couldn't add {nameof(PipeType)} '{Name}' to dictionary of all available PipeTypes.");
			}

			BatchRenderable = BatchRenderable.Create($"{Name} Pipe Type Batch Renderable", serializationData.GameData.GraphicsWorld, RenderingResources.DefaultPBRData, Mesh, RenderingResources.DefaultTexture, serializationData.GameData.GraphicsWorld.CameraProjViewBuffer, serializationData.GameData.GraphicsWorld.LightInfoBuffer);

			serializationData.GameData.GraphicsWorld.AddRenderable(BatchRenderable);
		}
	}
}
