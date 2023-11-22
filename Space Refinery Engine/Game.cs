using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Space_Refinery_Engine;

public record Game : ISerializableReference
{
	public Game(SerializableReference serializableReference, GameWorld gameWorld, Settings gameSettings)
	{
		SerializableReference = serializableReference;

		GameWorld = gameWorld;

		GameSettings = gameSettings;
	}

	private Game()
	{

	}

	public SerializableReference SerializableReference { get; private set; }

	public GameWorld GameWorld { get; private set; }

	public Settings GameSettings { get; private set; }

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(this);

		writer.SerializeWithoutEmbeddedType(GameWorld, nameof(GameWorld));

		writer.SerializeWithoutEmbeddedType(GameSettings, nameof(GameSettings));
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		SerializableReference = reader.ReadReference();

		GameWorld = reader.DeserializeEntitySerializableWithoutEmbeddedType<GameWorld>(serializationData, referenceHandler, nameof(GameWorld));

		GameSettings = reader.DeserializeEntitySerializableWithoutEmbeddedType<Settings>(serializationData, referenceHandler, nameof(GameSettings));
	}
}
