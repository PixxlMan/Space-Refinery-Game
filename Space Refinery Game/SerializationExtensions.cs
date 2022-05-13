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

		public static void Serialize(this Transform transform, XmlWriter writer)
		{
			writer.WriteStartElement(nameof(Transform));
			{
				writer.WriteStartElement(nameof(Transform.Position));
				{
					Serialize(transform.Position, writer);
				}
				writer.WriteEndElement();

				writer.WriteStartElement(nameof(Transform.Rotation));
				{
					Serialize(transform.Rotation, writer);
				}
				writer.WriteEndElement();

				writer.WriteStartElement(nameof(Transform.Scale));
				{
					Serialize(transform.Scale, writer);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public static Transform DeserializeTransform(this XmlReader reader)
		{
			reader.ReadStartElement(nameof(Transform));
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

		public static void Serialize(this Vector3FixedDecimalInt4 vector3FixedDecimalInt4, XmlWriter writer)
		{
			writer.WriteStartElement(nameof(Vector3FixedDecimalInt4));
			{
				Serialize(vector3FixedDecimalInt4.X, writer);
				Serialize(vector3FixedDecimalInt4.Y, writer);
				Serialize(vector3FixedDecimalInt4.Z, writer);

				writer.WriteEndElement();
			}
		}

		public static Vector3FixedDecimalInt4 DeserializeVector3FixedDecimalInt4(this XmlReader reader)
		{
			reader.ReadStartElement(nameof(Vector3FixedDecimalInt4));
			{
				Vector3FixedDecimalInt4 vector3FixedDecimalInt4;

				vector3FixedDecimalInt4 = new(reader.DeserializeFixedDecimalInt4(), reader.DeserializeFixedDecimalInt4(), reader.DeserializeFixedDecimalInt4());

				reader.ReadEndElement();

				return vector3FixedDecimalInt4;
			}

		}

		public static void Serialize(this QuaternionFixedDecimalInt4 quaternionFixedDecimalInt4, XmlWriter writer)
		{
			writer.WriteStartElement(nameof(QuaternionFixedDecimalInt4));
			{
				Serialize(quaternionFixedDecimalInt4.X, writer);
				Serialize(quaternionFixedDecimalInt4.Y, writer);
				Serialize(quaternionFixedDecimalInt4.Z, writer);
				Serialize(quaternionFixedDecimalInt4.W, writer);
			}
			writer.WriteEndElement();
		}

		public static QuaternionFixedDecimalInt4 DeserializeQuaternionFixedDecimalInt4(this XmlReader reader)
		{
			reader.ReadStartElement(nameof(QuaternionFixedDecimalInt4));
			{
				QuaternionFixedDecimalInt4 quaternionFixedDecimalInt4;

				quaternionFixedDecimalInt4 = new(reader.DeserializeFixedDecimalInt4(), reader.DeserializeFixedDecimalInt4(), reader.DeserializeFixedDecimalInt4(), reader.DeserializeFixedDecimalInt4());

				reader.ReadEndElement();

				return quaternionFixedDecimalInt4;
			}
		}

		public static void Serialize(this FixedDecimalInt4 fixedDecimalInt4, XmlWriter writer)
		{
			writer.WriteElementString(nameof(FixedDecimalInt4), fixedDecimalInt4.ToDecimal().ToString());
		}
		
		public static FixedDecimalInt4 DeserializeFixedDecimalInt4(this XmlReader reader)
		{
			reader.ReadStartElement(nameof(FixedDecimalInt4));
			{
				FixedDecimalInt4 result = FixedDecimalInt4.FromDecimal(decimal.Parse(reader.ReadContentAsString()));

				reader.ReadEndElement();

				return result;
			}
		}
	}
}
