using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using System.Xml;

namespace Space_Refinery_Engine;

public record struct Collider : IEntitySerializable
{
	public ColliderShapes Shape;
	public Vector3FixedDecimalInt4 Scale;
	public Transform Offset;

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		Shape = reader.DeserializeEnum<ColliderShapes>(nameof(Shape));
		Scale = reader.DeserializeVector3FixedDecimalInt4(nameof(Scale));
		Offset = reader.DeserializeTransform(nameof(Offset));
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.Serialize(Shape, nameof(Shape));
		writer.Serialize(Scale, nameof(Scale));
		writer.Serialize(Offset, nameof(Offset));
	}

	public IConvexShape GenerateConvexHull()
	{
		switch (Shape)
		{
			case ColliderShapes.Box:
				return new Box((float)Scale.X, (float)Scale.Y, (float)Scale.Z);
			case ColliderShapes.Cylinder:
				return new Cylinder((float)Scale.X, (float)Scale.Y);
			default:
				throw new Exception();
		}
	}
}

[Serializable]
public enum ColliderShapes
{
	Box,
	Cylinder,
}
