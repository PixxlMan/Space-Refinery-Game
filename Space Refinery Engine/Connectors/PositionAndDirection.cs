using FixedPrecision;
using System.Xml;

namespace Space_Refinery_Engine
{
	public struct PositionAndDirection : IEntitySerializable
	{
		public Vector3FixedDecimalInt4 Position;

		public Vector3FixedDecimalInt4 Direction;

		public void SerializeState(XmlWriter writer)
		{
			writer.Serialize(Position, nameof(Position));
			writer.Serialize(Direction, nameof(Direction));
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			Position = reader.DeserializeVector3FixedDecimalInt4(nameof(Position));
			Direction = reader.DeserializeVector3FixedDecimalInt4(nameof(Direction));
		}
	}
}
