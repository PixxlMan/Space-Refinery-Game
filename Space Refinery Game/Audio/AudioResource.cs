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

		public Guid SerializableReferenceGUID { get; private set; } = Guid.NewGuid();

		public void Play()
		{
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
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.Serialize(Name, nameof(Name));

			writer.Serialize(ResourcePath, nameof(ResourcePath));
		}
	}
}
