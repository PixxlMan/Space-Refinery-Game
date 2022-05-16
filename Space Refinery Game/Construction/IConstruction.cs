﻿using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Reflection;
using System.Xml;

namespace Space_Refinery_Game
{
	public interface IConstruction//<TConnector> where TConnector : Connector
	{
		public static abstract IConstruction/*<TConnector>*/ Build(/*TConnector*/ Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame);

		public void Deconstruct();

		public ConstructionInfo? ConstructionInfo { get; }

		public void Serialize(XmlWriter writer, Connector? sourceConnector, HashSet<IConstruction> serializedConstructions)
		{
			writer.WriteStartElement(nameof(IConstruction));
			{
				writer.WriteElementString("ConstructionType", GetType().AssemblyQualifiedName);

				SerializeImpl(writer, sourceConnector, serializedConstructions);
			}
			writer.WriteEndElement();
		}

		public void SerializeImpl(XmlWriter writer, Connector? sourceConnector, HashSet<IConstruction> serializedConstructions);

		public static abstract void DeserializeImpl(XmlReader reader, Connector? sourceConnector, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame);

		public delegate void DeserializeImplDelegate(XmlReader reader, Connector? sourceConnector, UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame);

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}