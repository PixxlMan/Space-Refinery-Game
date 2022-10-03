using System.Xml;

namespace Space_Refinery_Game
{
	public interface IEntitySerializable
	{
		public void SerializeState(XmlWriter writer);

		public void DeserializeState(XmlReader reader, GameData gameData, SerializationReferenceHandler referenceHandler);

		public static void SerializeWithoutEmbeddedType(XmlWriter writer, IEntitySerializable entitySerializable, string name = nameof(IEntitySerializable))
		{
			writer.WriteStartElement(name);
			{
				entitySerializable.SerializeState(writer);
			}
			writer.WriteEndElement();
		}

		public static void SerializeWithEmbeddedType(XmlWriter writer, IEntitySerializable entitySerializable, string name = nameof(IEntitySerializable))
		{
			writer.WriteStartElement(name);
			{
				writer.Serialize(entitySerializable.GetType());

				entitySerializable.SerializeState(writer);
			}
			writer.WriteEndElement();
		}

		public static T DeserializeWithoutEmbeddedType<T>(XmlReader reader, GameData gameData, SerializationReferenceHandler referenceHandler, string name = nameof(IEntitySerializable))
			where T : IEntitySerializable
		{
			T t = Activator.CreateInstance<T>();

			reader.ReadStartElement(name);
			{
				t.DeserializeState(reader, gameData, referenceHandler);
			}
			reader.ReadEndElement();

			return t;
		}

		public static IEntitySerializable DeserializeWithEmbeddedType(XmlReader reader, GameData gameData, SerializationReferenceHandler referenceHandler, string name = nameof(IEntitySerializable))
		{
			reader.ReadStartElement(name);
			{
				Type type = reader.DeserializeType();

				if (!type.IsAssignableTo(typeof(IEntitySerializable)))
				{
					throw new Exception($"Cannot deserialize object of type '{type.AssemblyQualifiedName}' as it does not inherit from {nameof(IEntitySerializable)}.");
				}

				IEntitySerializable entitySerializable = (IEntitySerializable)Activator.CreateInstance(type, true);

				entitySerializable.DeserializeState(reader, gameData, referenceHandler);

				reader.ReadEndElement();

				return entitySerializable;
			}
		}
	}

	public static class ExtensionsForIEntitySerializable
	{
		public static void SerializeWithoutEmbeddedType(this XmlWriter writer, IEntitySerializable entitySerializable)
			=> IEntitySerializable.SerializeWithoutEmbeddedType(writer, entitySerializable);

		public static T DeserializeEntitySerializableWithoutEmbeddedType<T>(this XmlReader reader, GameData gameData, SerializationReferenceHandler referenceHandler)
			where T : IEntitySerializable
			=> IEntitySerializable.DeserializeWithoutEmbeddedType<T>(reader, gameData, referenceHandler);
		
		public static void SerializeWithEmbeddedType(this XmlWriter writer, IEntitySerializable entitySerializable)
			=> IEntitySerializable.SerializeWithEmbeddedType(writer, entitySerializable);

		public static IEntitySerializable DeserializeEntitySerializableWithEmbeddedType(this XmlReader reader, GameData gameData, SerializationReferenceHandler referenceHandler)
			=> IEntitySerializable.DeserializeWithEmbeddedType(reader, gameData, referenceHandler);
	}
}
