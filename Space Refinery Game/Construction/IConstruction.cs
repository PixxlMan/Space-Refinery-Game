using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Reflection;
using System.Xml;

namespace Space_Refinery_Game
{
	public interface IConstruction//<TConnector> where TConnector : Connector
	{
		public static abstract IConstruction/*<TConnector>*/ Build(/*TConnector*/ Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame);

		public void Deconstruct();

/*		public sealed void Serialize(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(IConstruction));
			{
				writer.WriteElementString("Construction type", GetType().AssemblyQualifiedName);

				SerializeImpl(writer);
			}
			writer.WriteEndElement();
		}

		protected void SerializeImpl(XmlWriter writer);

		public sealed static IConstruction Deserialize(XmlReader reader)
		{
			IConstruction construction;

			reader.ReadStartElement(nameof(IConstruction));
			{
				string typeName;

				reader.ReadStartElement("Construction type");
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
					Type type = Type.GetType(typeName);

					MethodInfo deserializeImplMethod = type.GetMethod(nameof(DeserializeImpl));

					Func<XmlReader, IConstruction> func = (Func<XmlReader, IConstruction>)deserializeImplMethod.CreateDelegate(typeof(Func<XmlReader, IConstruction>));

					DeserializeImplMethods.Add(typeName, func);

					construction = func(reader);
				}
			}
			reader.ReadEndElement();

			return construction;
		}

		protected sealed static Dictionary<string, Func<XmlReader, IConstruction>> DeserializeImplMethods { get; set; } = new();

		protected static abstract IConstruction DeserializeImpl(XmlReader reader);*/

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}