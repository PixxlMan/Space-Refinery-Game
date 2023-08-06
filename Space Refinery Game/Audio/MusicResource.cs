using Space_Refinery_Utilities;
using System.Xml;

namespace Space_Refinery_Game.Audio
{
	/// <remarks>
	/// Not thread safe.
	/// </remarks>
	public class MusicResource : ISerializableReference
	{
		private MusicResource()
		{

		}

		public AudioWorld AudioWorld { get; private set; }

		public string Name { get; private set; }

		public SerializableReference SerializableReference { get; private set; } = Guid.NewGuid();

		public Track[] Tracks { get; private set; }

		public Track Intro { get; private set; }

		public List<Track> Loops { get; private set; } = new();

		public Track? Outro { get; private set; }

		public HashSet<MusicTag> Tags { get; private set; } = new();

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			AudioWorld = serializationData.GameData.AudioWorld;

			SerializableReference = reader.ReadReference();

			Name = reader.ReadString(nameof(Name));

			Track[] tracks = null;

			reader.DeserializeCollection((reader, i) => tracks[i] = reader.DeserializeEntitySerializableWithoutEmbeddedType<Track>(serializationData, referenceHandler, nameof(Track)), (count) => tracks = new Track[count], nameof(Tracks));
			
			reader.DeserializeCollection((reader) => Tags.AddUnique(reader.DeserializeEnum<MusicTag>("Tag"), "Duplicate tag"), nameof(Tags));

			AudioWorld.MusicSystem.RegisterMusic(this);

			serializationData.DeserializationCompleteEvent += () =>
			{
				Tracks = tracks;

				foreach (Track track in Tracks)
				{
					switch (track.MusicPart)
					{
						case MusicPart.Intro:
							Intro = track;
							break;
						case MusicPart.Loop:
							Loops.Add(track);
							break;
						case MusicPart.Outro:
							Outro = track;
							break;
						default:
							throw new GlitchInTheMatrixException();
					}
				}
			};
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Name, nameof(Name));

			writer.Serialize(Tracks, (writer, track) => writer.SerializeWithoutEmbeddedType(track, nameof(Track)), nameof(Tracks));

			writer.Serialize(Tags, (writer, tag) => writer.Serialize(tag, "Tag"), nameof(Tags));
		}
	}

	public class Track : IEntitySerializable
	{
		public AudioResource AudioResource { get; private set; }

		public MusicPart MusicPart { get; private set; }

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			//reader.ReadStartElement(nameof(Track));
			{
				reader.DeserializeReference<AudioResource>(referenceHandler, (ar) => AudioResource = ar, nameof(AudioResource));
				MusicPart = reader.DeserializeEnum<MusicPart>(nameof(MusicPart));
			}
			//reader.ReadEndElement();
		}

		public void SerializeState(XmlWriter writer)
		{
			//writer.WriteStartElement(nameof(Track));
			{
				writer.SerializeReference(AudioResource, nameof(AudioResource));
				writer.Serialize(MusicPart, nameof(MusicPart));
			}
			//writer.WriteEndElement();
		}
	}

	/// <summary>
	/// Describes how the audio in a track should be used in the conext of a song.
	/// </summary>
	public enum MusicPart
	{
		/// <summary>
		/// This audio is played once, at the start of the song. Should seamlessly transition from silence to any loop.
		/// </summary>
		Intro,
		/// <summary>
		/// This audio can be looped and combined with any other loop, plays after the intro but before the outro. Should seamlessly transition to any loop or the outro.
		/// </summary>
		Loop,
		/// <summary>
		/// This audio is played once, at the end of the song. Should seamlessly transition from any loop to silence.
		/// </summary>
		Outro
	}
}
