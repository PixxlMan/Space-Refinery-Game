using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public static class SerializationExtensions
	{
		public static MethodInfo GetImplementedMethod(this Type targetType, MethodInfo interfaceMethod) // https://stackoverflow.com/questions/1113635/how-to-get-methodinfo-of-interface-method-having-implementing-methodinfo-of-cla
		{
			if (targetType is null) throw new ArgumentNullException(nameof(targetType));
			if (interfaceMethod is null) throw new ArgumentNullException(nameof(interfaceMethod));

			var map = targetType.GetInterfaceMap(interfaceMethod.DeclaringType);
			var index = Array.IndexOf(map.InterfaceMethods, interfaceMethod);
			if (index < 0) return null;

			return map.TargetMethods[index];
		}

		public static void Serialize(this XmlWriter writer, Transform transform, string? name = null)
		{
			writer.WriteStartElement(name ?? nameof(Transform));
			{
				writer.WriteStartElement(nameof(Transform.Position));
				{
					Serialize(writer, transform.Position);
				}
				writer.WriteEndElement();

				writer.WriteStartElement(nameof(Transform.Rotation));
				{
					Serialize(writer, transform.Rotation);
				}
				writer.WriteEndElement();

				writer.WriteStartElement(nameof(Transform.Scale));
				{
					Serialize(writer, transform.Scale);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public static Transform DeserializeTransform(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(Transform));
			{
				Transform transform;

				Vector3FixedDecimalInt4 position;

				reader.ReadStartElement(nameof(Transform.Position));
					position = reader.DeserializeVector3FixedDecimalInt4();
				reader.ReadEndElement();

				QuaternionFixedDecimalInt4 rotation;
				
				reader.ReadStartElement(nameof(Transform.Rotation));
					rotation = reader.DeserializeQuaternionFixedDecimalInt4();
				reader.ReadEndElement();

				Vector3FixedDecimalInt4 scale;

				reader.ReadStartElement(nameof(Transform.Scale));
					scale = reader.DeserializeVector3FixedDecimalInt4();
				reader.ReadEndElement();

				transform = new(position, rotation, scale);

				reader.ReadEndElement();

				return transform;
			}
		}

		public static void Serialize(this XmlWriter writer, Vector3FixedDecimalInt4 vector3FixedDecimalInt4, string? name = null)
		{
			writer.WriteStartElement(name ?? nameof(Vector3FixedDecimalInt4));
			{
				Serialize(writer, vector3FixedDecimalInt4.X, "X");
				Serialize(writer, vector3FixedDecimalInt4.Y, "Y");
				Serialize(writer, vector3FixedDecimalInt4.Z, "Z");

				writer.WriteEndElement();
			}
		}

		public static Vector3FixedDecimalInt4 DeserializeVector3FixedDecimalInt4(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(Vector3FixedDecimalInt4));
			{
				Vector3FixedDecimalInt4 vector3FixedDecimalInt4;

				vector3FixedDecimalInt4 = new(reader.DeserializeFixedDecimalInt4("X"), reader.DeserializeFixedDecimalInt4("Y"), reader.DeserializeFixedDecimalInt4("Z"));

				reader.ReadEndElement();

				return vector3FixedDecimalInt4;
			}
		}

		public static void Serialize(this XmlWriter writer, QuaternionFixedDecimalInt4 quaternionFixedDecimalInt4, string? name = null)
		{
			writer.WriteStartElement(name ?? nameof(QuaternionFixedDecimalInt4));
			{
				Serialize(writer, quaternionFixedDecimalInt4.X, "X");
				Serialize(writer, quaternionFixedDecimalInt4.Y, "Y");
				Serialize(writer, quaternionFixedDecimalInt4.Z, "Z");
				Serialize(writer, quaternionFixedDecimalInt4.W, "W");
			}
			writer.WriteEndElement();
		}

		public static QuaternionFixedDecimalInt4 DeserializeQuaternionFixedDecimalInt4(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(QuaternionFixedDecimalInt4));
			{
				QuaternionFixedDecimalInt4 quaternionFixedDecimalInt4;

				quaternionFixedDecimalInt4 = new(reader.DeserializeFixedDecimalInt4("X"), reader.DeserializeFixedDecimalInt4("Y"), reader.DeserializeFixedDecimalInt4("Z"), reader.DeserializeFixedDecimalInt4("W"));

				reader.ReadEndElement();

				return quaternionFixedDecimalInt4;
			}
		}

		public static void Serialize(this XmlWriter writer, FixedDecimalInt4 fixedDecimalInt4, string? name = null)
		{
			writer.WriteElementString(name ?? nameof(FixedDecimalInt4), fixedDecimalInt4.ToDecimal().ToString());
		}
		
		public static FixedDecimalInt4 DeserializeFixedDecimalInt4(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(FixedDecimalInt4));
			{
				FixedDecimalInt4 result = FixedDecimalInt4.FromDecimal(reader.ReadContentAsDecimal());

				reader.ReadEndElement();

				return result;
			}
		}

		public static void Serialize(this XmlWriter writer, bool boolean, string? name = null)
		{
			writer.WriteElementString(name ?? nameof(Boolean), boolean.ToString());
		}
		
		public static bool DeserializeBoolean(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(FixedDecimalInt4));
			{
				bool boolean = bool.Parse(reader.ReadContentAsString());

				reader.ReadEndElement();

				return boolean;
			}
		}

		public static void Serialize(this XmlWriter writer, FixedDecimalLong8 fixedDecimalLong8, string? name = null)
		{
			writer.WriteElementString(name ?? nameof(FixedDecimalLong8), fixedDecimalLong8.ToDecimal().ToString());
		}

		public static FixedDecimalLong8 DeserializeFixedDecimalLong8(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(FixedDecimalLong8));
			{
				FixedDecimalLong8 result = FixedDecimalLong8.FromDecimal(reader.ReadContentAsDecimal());

				reader.ReadEndElement();

				return result;
			}
		}

		public static void Serialize<T>(this XmlWriter writer, ICollection<T> collection, Action<XmlWriter, T> serializationAction, string? name = null)
		{
			writer.WriteStartElement(name ?? "Collection");
			{
				writer.WriteElementString("Count", collection.Count.ToString());

				writer.WriteStartElement("Elements");
				{
					foreach (T serializable in collection)
					{
						serializationAction(writer, serializable);
					}
				}				
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public static ICollection<T> DeserializeCollection<T>(this XmlReader reader, Func<XmlReader, T> deserializationAction, string? name = null)
		{
			T[] array;

			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				array = new T[count];

				reader.ReadStartElement("Elements");
				{
					for (int i = 0; i < count; i++)
					{
						array[i] = deserializationAction(reader);
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();

			return array;
		}

		public static void SerializeReference(this XmlWriter writer, ISerializableReference serializableReference, string? name = null)
		{
			writer.WriteElementString(name ?? "GUID", serializableReference.SerializableReferenceGUID.ToString());
		}

		public static Guid ReadRefereceGUID(this XmlReader reader, string? name = null)
		{
			Guid guid = Guid.Parse(reader.ReadElementString(name ?? "GUID"));

			return guid;
		}

		public async static Task<ISerializableReference> DeserializeReference(this XmlReader reader, SerializationReferenceHandler referenceHandler, string? name = null)
		{
			Guid guid = Guid.Parse(reader.ReadElementString(name ?? "GUID"));

			return await referenceHandler.AwaitEventualReference(guid);
		}

		public async static Task<T> DeserializeReference<T>(this XmlReader reader, SerializationReferenceHandler referenceHandler, string? name = null)
			where T : ISerializableReference
		{
			return (T)await DeserializeReference(reader, referenceHandler, name);
		}
	}
}
