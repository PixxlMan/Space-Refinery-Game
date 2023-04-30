using SharpAudio.Codec;
using System.Collections.Generic;
using System.Xml;
using Vortice.Direct3D11;

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

		public AudioResource[] Tracks { get; private set; }

		public HashSet<MusicTag> Tags { get; private set; } = new();

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			AudioWorld = serializationData.GameData.AudioWorld;

			SerializableReferenceGUID = reader.ReadReferenceGUID();

			Name = reader.ReadString(nameof(Name));

			AudioResource[] tracks = null;

			reader.DeserializeCollection((reader, i) => reader.DeserializeReference<AudioResource>(referenceHandler, (audioResource) => tracks[i] = audioResource), (count) => tracks = new AudioResource[count], nameof(Tracks));
			
			reader.DeserializeCollection((reader) => Tags.AddUnique(reader.DeserializeEnum<MusicTag>("Tag"), "Duplicate tag"), nameof(Tags));

			AudioWorld.MusicSystem.RegisterMusic(this);

			serializationData.DeserializationCompleteEvent += () =>
			{
				Tracks = tracks;
			};
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Name, nameof(Name));

			writer.Serialize(Tracks, nameof(Tracks));

			writer.Serialize(Tags, (writer, tag) => writer.Serialize(tag, "Tag"), nameof(Tags));
		}
	}

	/*public struct Track : IEntitySerializable
	{
		public AudioResource AudioResource { get; private set; }

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			throw new NotImplementedException();
		}

		public void SerializeState(XmlWriter writer)
		{
			throw new NotImplementedException();
		}
	}*/
}
