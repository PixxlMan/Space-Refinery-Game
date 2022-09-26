using Space_Refinery_Game_Renderer;
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
		public static void Deserialize(XmlReader reader, Connector? sourceConnector, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
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
					DeserializeImplMethods[typeName](reader, sourceConnector, ui, physicsWorld, graphicsWorld, gameWorld, mainGame, referenceHandler);
				}
				else
				{
					Type type = Type.GetType(typeName, throwOnError: true);

					if (!type.IsAssignableTo(typeof(IConstruction)))
					{
						throw new Exception($"Type {type.Name} does not implement {nameof(IConstruction)}.");
					}

					MethodInfo? deserializeImplMethod = type.GetImplementedMethod(DeserializeImplInterfaceMethod);

					IConstruction.DeserializeImplDelegate func = (IConstruction.DeserializeImplDelegate)deserializeImplMethod.CreateDelegate(typeof(IConstruction.DeserializeImplDelegate));

					DeserializeImplMethods.Add(typeName, func);

					func(reader, sourceConnector, ui, physicsWorld, graphicsWorld, gameWorld, mainGame, referenceHandler);
				}
			}
			reader.ReadEndElement();
		}

		private static MethodInfo DeserializeImplInterfaceMethod = typeof(IConstruction).GetMethod(nameof(IConstruction.DeserializeImpl));

		private static Dictionary<string, IConstruction.DeserializeImplDelegate> DeserializeImplMethods { get; set; } = new();
	}
}
