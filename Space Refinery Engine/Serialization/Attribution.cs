using System.Collections.Concurrent;
using System.Xml;

namespace Space_Refinery_Engine;

public enum AttributionType
{
	Asset,
	Role,
}

public sealed class Attribution() : ISerializableReference
{
	public static ConcurrentBag<Attribution> Attributions = new();

	public AttributionType AttributionType { get; private set; }

	public string AttributionSection { get; private set; }

	public string AttributionTarget { get; private set; }

	public string AttributedParty { get; private set; }

	public string AttributionLink { get; private set; }

	private readonly Guid guid = Guid.NewGuid();

	public SerializableReference SerializableReference => guid;

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		AttributionType = reader.DeserializeEnum<AttributionType>(nameof(AttributionType));
		AttributionSection = reader.ReadString(nameof(AttributionSection));
		AttributionTarget = reader.ReadString(nameof(AttributionTarget));
		AttributedParty = reader.ReadString(nameof(AttributedParty));
		AttributionLink = reader.ReadString(nameof(AttributionLink));

		Attributions.Add(this);
	}

	public void SerializeState(XmlWriter writer)
	{
		throw new NotSupportedException();
	}

	public override string ToString()
	{
		return $"{AttributionSection}: {AttributionTarget} from {AttributedParty}, {AttributionLink}";
	}
}
