using FixedPrecision;
using SharpAudio.Codec;
using System.Xml;

namespace Space_Refinery_Game.Audio
{
	public class AudioResource : ISerializableReference
	{
		private AudioResource()
		{
			
		}

		public AudioWorld AudioWorld { get; private set; }
		
		public SoundStream SoundStream { get; private set; }

		public string ResourcePath { get; private set; }

		public string Name { get; private set; }

		public FixedDecimalLong8 ClipVolume { get; private set; }

		public Guid SerializableReferenceGUID { get; private set; } = Guid.NewGuid();

		public void Play(FixedDecimalLong8 playbackVolume)
		{
			SoundStream.Volume = (float)(AudioWorld.MasterVolume * ClipVolume * playbackVolume);
			SoundStream.Play();
		}

		public void PlayAbsolute(FixedDecimalLong8 absoluteVolume)
		{
			SoundStream.Volume = (float)absoluteVolume;
			SoundStream.Play();
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			AudioWorld = serializationData.GameData.AudioWorld;

			SerializableReferenceGUID = reader.ReadReferenceGUID();

			Name = reader.ReadString(nameof(Name));

			ResourcePath = reader.ReadString(nameof(ResourcePath));

			Logging.Log($"Streaming audio file '{Name}' from path '{Path.GetFullPath(ResourcePath)}'.");

			SoundStream = new(File.OpenRead(ResourcePath), serializationData.GameData.AudioWorld.AudioEngine);

			ClipVolume = reader.DeserializeFixedDecimalLong8(nameof(ClipVolume));
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Name, nameof(Name));

			writer.Serialize(ResourcePath, nameof(ResourcePath));

			writer.Serialize(ClipVolume, nameof(ClipVolume));
		}
	}
}
