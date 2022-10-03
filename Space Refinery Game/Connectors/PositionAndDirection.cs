using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
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

		public void DeserializeState(XmlReader reader, GameData gameData, SerializationReferenceHandler referenceHandler)
		{
			Position = reader.DeserializeVector3FixedDecimalInt4(nameof(Position));
			Direction = reader.DeserializeVector3FixedDecimalInt4(nameof(Direction));
		}
	}
}
