using FixedPrecision;
using System.Xml;

namespace Space_Refinery_Game
{
	public struct PipeProperties : IEntitySerializable
	{
		public PipeShape Shape;

		//public FixedDecimalLong8 WallInsulation;

		//public FixedDecimalLong8 FlowableRadius;

		//public FixedDecimalLong8 FlowableLength;

		public FixedDecimalLong8 FlowableVolume;

		public FixedDecimalLong8 Friction;

		public PipeProperties(PipeShape shape, /*FixedDecimalLong8 wallInsulation, FixedDecimalLong8 flowableRadius, FixedDecimalLong8 flowableLength,*/ FixedDecimalLong8 flowableVolume, FixedDecimalLong8 friction)
		{
			Shape = shape;
			/*WallInsulation = wallInsulation;
			FlowableRadius = flowableRadius;
			FlowableLength = flowableLength;*/
			FlowableVolume = flowableVolume;
			Friction = friction;
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.Serialize(Shape, nameof(Shape));

			/*writer.Serialize(WallInsulation, nameof(WallInsulation));
			writer.Serialize(FlowableRadius, nameof(FlowableRadius));
			writer.Serialize(FlowableLength, nameof(FlowableLength));*/
			writer.Serialize(FlowableVolume, nameof(FlowableVolume));
			writer.Serialize(Friction, nameof(Friction));
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			Shape = reader.DeserializeEnum<PipeShape>(nameof(Shape));

			/*WallInsulation = reader.DeserializeFixedDecimalLong8(nameof(WallInsulation));
			FlowableRadius = reader.DeserializeFixedDecimalLong8(nameof(FlowableRadius));
			FlowableLength = reader.DeserializeFixedDecimalLong8(nameof(FlowableLength));*/
			FlowableVolume = reader.DeserializeFixedDecimalLong8(nameof(FlowableVolume));
			Friction = reader.DeserializeFixedDecimalLong8(nameof(Friction));
		}
	}
}