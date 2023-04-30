using SharpAudio.Codec;
using System.Collections.Generic;
using System.Xml;

namespace Space_Refinery_Game.Audio
{
	public class MusicResource : ISerializableReference
	{
		private MusicResource()
		{

		}

		public AudioWorld AudioWorld { get; private set; }

		public string Name { get; private set; }

		public Guid SerializableReferenceGUID { get; private set; } = Guid.NewGuid();

		public List<AudioResource> Tracks { get; private set; } = new();

		public List<MusicTag> Tags { get; private set; } = new();

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			AudioWorld = serializationData.GameData.AudioWorld;

			SerializableReferenceGUID = reader.ReadReferenceGUID();

			Name = reader.ReadString(nameof(Name));

			reader.DeserializeReferenceCollection(Tracks, referenceHandler, nameof(Tracks));

			Tags.AddRange(reader.DeserializeCollection((reader) => reader.DeserializeEnum<MusicTag>("Tag"), nameof(Tags)));

			AudioWorld.MusicSystem.RegisterMusic(this);
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Name, nameof(Name));

			writer.Serialize(Tracks, nameof(Tracks));

			writer.Serialize(Tags, (writer, tag) => writer.Serialize(tag, "Tag"), nameof(Tags));
		}
	}
}
