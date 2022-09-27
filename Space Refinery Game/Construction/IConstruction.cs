using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Reflection;
using System.Xml;

namespace Space_Refinery_Game
{
	public interface IConstruction : ISerializableReference//<TConnector> where TConnector : Connector
	{
		public static abstract IConstruction/*<TConnector>*/ Build(/*TConnector*/ Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler);

		public void Deconstruct();

		public ConstructionInfo? ConstructionInfo { get; }

		public void Serialize(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(IConstruction));
			{
				writer.WriteElementString("ConstructionType", GetType().AssemblyQualifiedName);

				SerializeImpl(writer);
			}
			writer.WriteEndElement();
		}

		public void SerializeImpl(XmlWriter writer);

		public static abstract void DeserializeImpl(XmlReader reader, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler);

		public delegate void DeserializeImplDelegate(XmlReader reader, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler);

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}