using Singulink.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;

namespace Space_Refinery_Game
{
	public interface IEntitySerializable
	{
		public void SerializeState(XmlWriter writer);

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler);

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

		public static T DeserializeWithoutEmbeddedType<T>(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler, string name = nameof(IEntitySerializable))
			where T : IEntitySerializable
		{
			T entitySerializable;

			if (typeof(T) == typeof(ValueType))
			{
				entitySerializable = (T)FormatterServices.GetUninitializedObject(typeof(T));
			}
			else
			{
				entitySerializable = ObjectFactory.CreateInstance<T>();
			}

			reader.ReadStartElement(name);
			{
				entitySerializable.DeserializeState(reader, serializationData, referenceHandler);
			}
			reader.ReadEndElement();

			return entitySerializable;
		}

		public static IEntitySerializable DeserializeWithEmbeddedType(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler, string name = nameof(IEntitySerializable))
		{
			reader.ReadStartElement(name);
			{
				Type type = reader.DeserializeType();

				if (!type.IsAssignableTo(typeof(IEntitySerializable)))
				{
					throw new Exception($"Cannot deserialize object of type '{type.AssemblyQualifiedName}' as it does not inherit from {nameof(IEntitySerializable)}.");
				}

				IEntitySerializable entitySerializable;

				if (type.IsValueType)
				{
					entitySerializable = (IEntitySerializable)FormatterServices.GetUninitializedObject(type);
				}
				else
				{
					entitySerializable = (IEntitySerializable)ObjectFactory.CreateInstance(type, true);
				}

				entitySerializable.DeserializeState(reader, serializationData, referenceHandler);

				reader.ReadEndElement();

				return entitySerializable;
			}
		}
	}

	public static class ExtensionsForIEntitySerializable
	{
		public static void SerializeWithoutEmbeddedType(this XmlWriter writer, IEntitySerializable entitySerializable)
			=> IEntitySerializable.SerializeWithoutEmbeddedType(writer, entitySerializable);

		public static T DeserializeEntitySerializableWithoutEmbeddedType<T>(this XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
			where T : IEntitySerializable
			=> IEntitySerializable.DeserializeWithoutEmbeddedType<T>(reader, serializationData, referenceHandler);

		public static void SerializeWithEmbeddedType(this XmlWriter writer, IEntitySerializable entitySerializable)
			=> IEntitySerializable.SerializeWithEmbeddedType(writer, entitySerializable);

		public static IEntitySerializable DeserializeEntitySerializableWithEmbeddedType(this XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
			=> IEntitySerializable.DeserializeWithEmbeddedType(reader, serializationData, referenceHandler);


		public static void SerializeWithoutEmbeddedType(this XmlWriter writer, IEntitySerializable entitySerializable, string name)
			=> IEntitySerializable.SerializeWithoutEmbeddedType(writer, entitySerializable, name);

		public static T DeserializeEntitySerializableWithoutEmbeddedType<T>(this XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler, string name)
			where T : IEntitySerializable
			=> IEntitySerializable.DeserializeWithoutEmbeddedType<T>(reader, serializationData, referenceHandler, name);
		
		public static void SerializeWithEmbeddedType(this XmlWriter writer, IEntitySerializable entitySerializable, string name)
			=> IEntitySerializable.SerializeWithEmbeddedType(writer, entitySerializable, name);

		public static IEntitySerializable DeserializeEntitySerializableWithEmbeddedType(this XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler, string name)
			=> IEntitySerializable.DeserializeWithEmbeddedType(reader, serializationData, referenceHandler, name);
	}
}
