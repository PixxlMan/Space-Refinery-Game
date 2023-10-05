using FixedPrecision;
using System.Xml;

namespace Space_Refinery_Engine.Audio
{
	/// <summary>
	/// Represents a serializable auditory resource.
	/// </summary>
	/// <remarks>
	/// Not thread safe.
	/// </remarks>
	public class AudioResource : ISerializableReference
	{
		private AudioResource()
		{
			
		}

		public AudioWorld AudioWorld { get; private set; }

		public string ResourcePath { get; private set; }

		public string Name { get; private set; }

		public FixedDecimalLong8 ClipVolume { get; private set; }

		public SerializableReference SerializableReference { get; private set; } = Guid.NewGuid();

		public AudioClipPlayback CreatePlayback()
		{
			return new AudioClipPlayback(ResourcePath);
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			AudioWorld = serializationData.GameData.AudioWorld;

			SerializableReference = reader.ReadReference();

			Name = reader.ReadString(nameof(Name));

			ResourcePath = reader.ReadString(nameof(ResourcePath));

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
