using FixedPrecision;
using System.Xml;

namespace Space_Refinery_Game
{
	public struct PipeProperties : IEntitySerializable
	{
		public PipeShape Shape;

		//public DecimalNumber WallInsulation;

		//public DecimalNumber FlowableRadius;

		//public DecimalNumber FlowableLength;

		public DecimalNumber FlowableVolume;

		public DecimalNumber Friction;

		public PipeProperties(PipeShape shape, /*DecimalNumber wallInsulation, DecimalNumber flowableRadius, DecimalNumber flowableLength,*/ DecimalNumber flowableVolume, DecimalNumber friction)
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

			/*WallInsulation = reader.DeserializeDecimalNumber(nameof(WallInsulation));
			FlowableRadius = reader.DeserializeDecimalNumber(nameof(FlowableRadius));
			FlowableLength = reader.DeserializeDecimalNumber(nameof(FlowableLength));*/
			FlowableVolume = reader.DeserializeDecimalNumber(nameof(FlowableVolume));
			Friction = reader.DeserializeDecimalNumber(nameof(Friction));
		}
	}
}