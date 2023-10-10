﻿using System.Xml;

namespace Space_Refinery_Engine
{
	[Serializable]
	public sealed class PipeConnectorProperties : ISerializableReference
	{
		public PipeShape Shape;

		public DN ConnectorDiameter;

		public DN ConnectorFlowAreaDiameter;

		public PipeConnectorProperties(PipeShape shape, DN connectorDiameter, DN connectorFlowAreaDiameter, SerializationReferenceHandler referenceHandler)
		{
			Shape = shape;
			ConnectorDiameter = connectorDiameter;
			ConnectorFlowAreaDiameter = connectorFlowAreaDiameter;

			SerializableReference = Guid.NewGuid();

			referenceHandler.RegisterReference(this);
		}

		private PipeConnectorProperties()
		{

		}

		public SerializableReference SerializableReference { get; private set; }

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Shape, nameof(Shape));

			writer.Serialize(ConnectorDiameter, nameof(ConnectorDiameter));

			writer.Serialize(ConnectorFlowAreaDiameter, nameof(ConnectorFlowAreaDiameter));
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SerializableReference = reader.ReadReference();

			Shape = reader.DeserializeEnum<PipeShape>(nameof(Shape));

			ConnectorDiameter = reader.DeserializeDecimalNumber(nameof(ConnectorDiameter));

			ConnectorFlowAreaDiameter = reader.DeserializeDecimalNumber(nameof(ConnectorFlowAreaDiameter));
		}
	}
}