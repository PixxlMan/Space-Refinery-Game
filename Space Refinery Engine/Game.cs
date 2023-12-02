using Microsoft.Toolkit.HighPerformance;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Space_Refinery_Engine;

public record Game : ISerializableReference
{
	private Game()
	{  }

	private SerializableReference serializableReference;
	public SerializableReference SerializableReference
	{
		get
		{
			lock (syncRoot)
			{
				return serializableReference;
			}
		}
		private set
		{
			lock (syncRoot)
			{
				serializableReference = value;
			}
		}
	}

	public SerializationReferenceHandler GameReferenceHandler { get; private set; }

	public GameWorld GameWorld { get; private set; }

	public Settings GameSettings { get; private set; }

	public Player Player { get; private set; }

	private object syncRoot = new();

	public static Game CreateGame(SerializableReference gameName, GameData gameData)
	{
		Game game = new()
		{
			SerializableReference = gameName,
			GameWorld = new(),
			GameSettings = new(gameData),
			GameReferenceHandler = new(),
			Player = Player.Create(gameData),
		};

		return game;
	}

	public void SerializeState(XmlWriter writer)
	{
		lock (syncRoot)
		{
			writer.WriteStartElement(nameof(Game));
			{
				writer.SerializeReference(this);

				GameReferenceHandler.Serialize(writer);

				writer.SerializeWithoutEmbeddedType(GameWorld, nameof(GameWorld));

				writer.SerializeWithoutEmbeddedType(GameSettings, nameof(GameSettings));

				Player.Serialize(writer);
			}
			writer.WriteEndElement();
		}
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, [MaybeNull]SerializationReferenceHandler _ /*Will always be null since this method is what deserializes the SerializableReferenceHandler to begin with!*/)
	{
		lock (syncRoot)
		{
			reader.ReadStartElement(nameof(Game));
			{
				SerializableReference = reader.ReadReference();

				GameReferenceHandler = SerializationReferenceHandler.Deserialize(reader, serializationData, false);

				GameWorld = reader.DeserializeEntitySerializableWithoutEmbeddedType<GameWorld>(serializationData, GameReferenceHandler, nameof(GameWorld));

				GameSettings = reader.DeserializeEntitySerializableWithoutEmbeddedType<Settings>(serializationData, GameReferenceHandler, nameof(GameSettings));

				Player = Player.Deserialize(reader, serializationData);

				GameReferenceHandler.ExitAllowEventualReferenceMode();
			}
			reader.ReadEndElement();
		}
	}
}
