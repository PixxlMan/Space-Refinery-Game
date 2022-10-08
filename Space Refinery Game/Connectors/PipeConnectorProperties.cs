using FixedPrecision;
using System.Xml;

namespace Space_Refinery_Game
{
	[Serializable]
	public sealed class PipeConnectorProperties : ISerializableReference
	{
		public PipeShape Shape;

		public DecimalNumber ConnectorDiameter;

		public DecimalNumber ConnectorFlowAreaDiameter;

		public PipeConnectorProperties(PipeShape shape, DecimalNumber connectorDiameter, DecimalNumber connectorFlowAreaDiameter, SerializationReferenceHandler referenceHandler)
		{
			Shape = shape;
			ConnectorDiameter = connectorDiameter;
			ConnectorFlowAreaDiameter = connectorFlowAreaDiameter;

			SerializableReferenceGUID = Guid.NewGuid();

			referenceHandler.RegisterReference(this);
		}

		private PipeConnectorProperties()
		{

		}

		public Guid SerializableReferenceGUID { get; private set; }

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Shape, nameof(Shape));

			writer.Serialize(ConnectorDiameter, nameof(ConnectorDiameter));

			writer.Serialize(ConnectorFlowAreaDiameter, nameof(ConnectorFlowAreaDiameter));
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SerializableReferenceGUID = reader.ReadReferenceGUID();

			Shape = reader.DeserializeEnum<PipeShape>(nameof(Shape));

			ConnectorDiameter = reader.DeserializeDecimalNumber(nameof(ConnectorDiameter));

			ConnectorFlowAreaDiameter = reader.DeserializeDecimalNumber(nameof(ConnectorFlowAreaDiameter));
		}
	}
}