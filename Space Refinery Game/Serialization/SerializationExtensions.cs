#if DEBUG
#define IncludeUnits
#endif

using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using Space_Refinery_Utilities.Units;

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

		public static void Serialize(this XmlWriter writer, string text, string name = "String")
		{
			writer.WriteElementString(name, text);
		}

		public static void Serialize(this XmlWriter writer, Type type, string name = "Type")
		{
			writer.WriteElementString(name, type.FullName);
		}

		public static Type DeserializeType(this XmlReader reader, string name = "Type")
		{
			return Type.GetType(reader.ReadString(name), true);
		}

		public static void Serialize(this XmlWriter writer, Enum @enum, string name = "Enum")
		{
			writer.WriteElementString(name, @enum.ToString());
		}

		public static T DeserializeEnum<T>(this XmlReader reader, string name = "Enum")
			where T : struct, Enum
		{
			return Enum.Parse<T>(reader.ReadString(name));
		}

		public static string ReadString(this XmlReader reader, string? name = null)
		{
			string value;

			if (name is null)
			{
				reader.ReadStartElement();
				{
					value = reader.ReadString();
				}
				reader.ReadEndElement();
			}
			else
			{
				reader.ReadStartElement(name);
				{
					value = reader.ReadString();
				}
				reader.ReadEndElement();
			}

			return value;
		}

		public static void Serialize(this XmlWriter writer, Transform transform, string? name = null)
		{
			writer.WriteStartElement(name ?? nameof(Transform));
			{
				Serialize(writer, transform.Position, nameof(Transform.Position));

				Serialize(writer, transform.Rotation, nameof(Transform.Rotation));
			}
			writer.WriteEndElement();
		}

		public static Transform DeserializeTransform(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(Transform));
			{
				Transform transform;

				Vector3FixedDecimalInt4 position = reader.DeserializeVector3FixedDecimalInt4(nameof(Transform.Position));

				QuaternionFixedDecimalInt4 rotation = reader.DeserializeQuaternionFixedDecimalInt4(nameof(Transform.Rotation));

				transform = new(position, rotation);

				reader.ReadEndElement();

				return transform;
			}
		}

		public static void Serialize(this XmlWriter writer, Vector3FixedDecimalInt4 Vector3FixedDecimalInt4, string? name = null)
		{
			writer.WriteStartElement(name ?? nameof(Vector3FixedDecimalInt4));
			{
				Serialize(writer, Vector3FixedDecimalInt4.X, "X");
				Serialize(writer, Vector3FixedDecimalInt4.Y, "Y");
				Serialize(writer, Vector3FixedDecimalInt4.Z, "Z");

				writer.WriteEndElement();
			}
		}

		public static Vector3FixedDecimalInt4 DeserializeVector3FixedDecimalInt4(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(Vector3FixedDecimalInt4));
			{
				Vector3FixedDecimalInt4 Vector3FixedDecimalInt4;

				Vector3FixedDecimalInt4 = new(reader.DeserializeDecimalNumber("X"), reader.DeserializeDecimalNumber("Y"), reader.DeserializeDecimalNumber("Z"));

				reader.ReadEndElement();

				return Vector3FixedDecimalInt4;
			}
		}

		public static void Serialize(this XmlWriter writer, QuaternionFixedDecimalInt4 QuaternionFixedDecimalInt4, string? name = null)
		{
			writer.WriteStartElement(name ?? nameof(QuaternionFixedDecimalInt4));
			{
				Serialize(writer, QuaternionFixedDecimalInt4.X, "X");
				Serialize(writer, QuaternionFixedDecimalInt4.Y, "Y");
				Serialize(writer, QuaternionFixedDecimalInt4.Z, "Z");
				Serialize(writer, QuaternionFixedDecimalInt4.W, "W");
			}
			writer.WriteEndElement();
		}

		public static QuaternionFixedDecimalInt4 DeserializeQuaternionFixedDecimalInt4(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(QuaternionFixedDecimalInt4));
			{
				QuaternionFixedDecimalInt4 quaternionFixedDecimalInt4;

				quaternionFixedDecimalInt4 = new(reader.DeserializeDecimalNumber("X"), reader.DeserializeDecimalNumber("Y"), reader.DeserializeDecimalNumber("Z"), reader.DeserializeDecimalNumber("W"));

				reader.ReadEndElement();

				return quaternionFixedDecimalInt4;
			}
		}

		public static void Serialize(this XmlWriter writer, DecimalNumber decimalNumber, string? name = null)
		{
			writer.WriteElementString(name ?? nameof(decimalNumber), decimalNumber.ToDecimal().ToString());
		}
		
		public static DecimalNumber DeserializeDecimalNumber(this XmlReader reader, string? name = null)
		{
			reader.ReadStartElement(name ?? nameof(DecimalNumber));
			{
				string text = reader.ReadString();

				decimal value = decimal.Parse(text, DecimalNumber.NumberFormat);

				DecimalNumber result = DecimalNumber.FromDecimal(value);

				reader.ReadEndElement();

				return result;
			}
		}

		public static void Serialize<TUnit>(this XmlWriter writer, TUnit unit, string? name = null)
#if IncludeUnits
			where TUnit :
				IUnit<TUnit>,
				IPortionable<TUnit>,
				IIntervalSupport<TUnit>
#endif
		{
			writer.WriteElementString(name ?? nameof(TUnit), ((DecimalNumber)unit).ToDecimal().ToString());
		}
		
		public static TUnit DeserializeUnit<TUnit>(this XmlReader reader, string? name = null)
#if IncludeUnits
			where TUnit :
				IUnit<TUnit>,
				IPortionable<TUnit>,
				IIntervalSupport<TUnit>
#endif
		{
			reader.ReadStartElement(name ?? nameof(TUnit));
			{
				string text = reader.ReadString();

				decimal value = decimal.Parse(text, DecimalNumber.NumberFormat);

				TUnit result = (TUnit)DecimalNumber.FromDecimal(value);

				reader.ReadEndElement();

				return result;
			}
		}

		public static void Serialize(this XmlWriter writer, FixedDecimalInt4 fixedDecimalInt4, string? name = null)
		{
			writer.WriteElementString(name ?? nameof(FixedDecimalInt4), fixedDecimalInt4.ToDecimal().ToString());
		}

		public static FixedDecimalInt4 DeserializeFixedDecimalInt4(this XmlReader reader, string name = nameof(FixedDecimalInt4))
		{
			reader.ReadStartElement(name);
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
			reader.ReadStartElement(name ?? nameof(DecimalNumber));
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

		public static void Serialize<T>(this XmlWriter writer, ICollection<T> collection, string? name = null)
			where T : ISerializableReference
		{
			writer.WriteStartElement(name ?? "Collection");
			{
				writer.WriteElementString("Count", collection.Count.ToString());

				writer.WriteStartElement("Elements");
				{
					foreach (T serializable in collection)
					{
						writer.SerializeReference(serializable);
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
					if (count == 0)
					{
						reader.ReadEndElement();

						return Array.Empty<T>();
					}

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
		
		public static ICollection<T> DeserializeCollection<T>(this XmlReader reader, Action<XmlReader, Action<T>> deserializationAction, string? name = null)
		{
			T[] array;

			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				array = new T[count];

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement();

						return Array.Empty<T>();
					}

					for (int i = 0; i < count; i++)
					{						
						deserializationAction(reader, (t) => array[i] = t);
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();

			return array;
		}
		
		public static ICollection<T> DeserializeCollection<T>(this XmlReader reader, Action<XmlReader, Action<T>, int> deserializationAction, string? name = null)
		{
			T[] array;

			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				array = new T[count];

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement();

						return Array.Empty<T>();
					}

					for (int i = 0; i < count; i++)
					{
						int iCopy = i;
						deserializationAction(reader, (t) => array[iCopy] = t, iCopy);
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();

			return array;
		}

		public static void DeserializeCollection(this XmlReader reader, Action<XmlReader> deserializationAction, string? name = null)
		{
			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement();

						return;
					}

					for (int i = 0; i < count; i++)
					{
						deserializationAction(reader);
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}
		
		public static void DeserializeCollection(this XmlReader reader, Action<XmlReader, int> deserializationAction, Action<int> collectionSizeKnown, string? name = null)
		{
			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				collectionSizeKnown.Invoke(count);

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement(); // shouldn't it be two read end elements?

						return;
					}

					for (int i = 0; i < count; i++)
					{
						deserializationAction(reader, i);
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}

		// This is necessary to allow deserializing to a concurrent bag.
		public static void DeserializeReferenceCollection<T>(this XmlReader reader, ConcurrentBag<T> concurrentBag, SerializationReferenceHandler referenceHandler, string? name = null)
			where T : ISerializableReference
		{
			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement();

						return;
					}

					for (int i = 0; i < count; i++)
					{
						DeserializeReference(reader, referenceHandler, (s) => concurrentBag.Add((T)s));
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}

		// The order of the returned objects is uncertain due to the unordered nature of DeserializeReference.
		public static void DeserializeReferenceCollectionUnordered<T>(this XmlReader reader, ICollection<T> collectionToAddTo, SerializationReferenceHandler referenceHandler, string? name = null)
			where T : ISerializableReference
		{
			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement();

						return;
					}

					for (int i = 0; i < count; i++)
					{
						DeserializeReference(reader, referenceHandler, (s) => collectionToAddTo.Add((T)s));
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}

		public static void DeserializeCollection(this XmlReader reader, Action<XmlReader, int> deserializationAction, string? name = null)
		{
			reader.ReadStartElement(name ?? "Collection");
			{
				int count = int.Parse(reader.ReadElementString("Count"));

				reader.ReadStartElement("Elements");
				{
					if (count == 0)
					{
						reader.ReadEndElement();

						return;
					}

					for (int i = 0; i < count; i++)
					{
						deserializationAction(reader, i);
					}
				}
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}

		public static void SerializeReference(this XmlWriter writer, ISerializableReference serializableReference, string name = "Reference")
		{
			writer.WriteElementString(name, serializableReference.SerializableReference.ToString());
		}

		public static SerializableReference ReadReference(this XmlReader reader, string name = "Reference")
		{
			SerializableReference serializableReference = SerializableReference.ParseString(reader.ReadElementString(name));

			return serializableReference;
		}

		public static void DeserializeReference(this XmlReader reader, SerializationReferenceHandler referenceHandler, Action<ISerializableReference> referenceRegisteredCallback, string name = "Reference")
		{
			referenceHandler.GetEventualReference(ReadReference(reader, name), referenceRegisteredCallback);
		}

		public static void DeserializeReference<T>(this XmlReader reader, SerializationReferenceHandler referenceHandler, Action<T> refrenceRegisteredCallback, string? name = "Reference")
			where T : ISerializableReference
		{
			DeserializeReference(reader, referenceHandler, (s) => refrenceRegisteredCallback((T)s), name);
		}
	}
}
