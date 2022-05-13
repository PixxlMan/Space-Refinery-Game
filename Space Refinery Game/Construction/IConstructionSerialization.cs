using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public static class IConstructionSerialization
	{
		public static IConstruction Deserialize(XmlReader reader)
		{
			IConstruction construction;

			reader.ReadStartElement(nameof(IConstruction));
			{
				string typeName;

				reader.ReadStartElement("ConstructionType");
				{
					typeName = reader.ReadContentAsString();
				}
				reader.ReadEndElement();

				if (DeserializeImplMethods.ContainsKey(typeName))
				{
					construction = DeserializeImplMethods[typeName](reader);
				}
				else
				{
					Type type = Type.GetType(typeName, throwOnError: true);

					if (!type.IsAssignableTo(typeof(IConstruction)))
					{
						throw new Exception($"Type {type.Name} does not implement {nameof(IConstruction)}.");
					}

					MethodInfo? deserializeImplMethod = type.GetImplementedMethod(DeserializeImplInterfaceMethod);

					Func<XmlReader, IConstruction> func = (Func<XmlReader, IConstruction>)deserializeImplMethod.CreateDelegate(typeof(Func<XmlReader, IConstruction>));

					DeserializeImplMethods.Add(typeName, func);

					construction = func(reader);
				}
			}
			reader.ReadEndElement();

			return construction;
		}

		private static MethodInfo DeserializeImplInterfaceMethod = typeof(IConstruction).GetMethod(nameof(IConstruction.DeserializeImpl));

		private static Dictionary<string, Func<XmlReader, IConstruction>> DeserializeImplMethods { get; set; } = new();
	}
}
