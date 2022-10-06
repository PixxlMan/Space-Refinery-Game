using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Veldrid;

namespace Space_Refinery_Game
{
	public abstract class Connector : Entity, ISerializableReference, IEntitySerializable
	{
		protected Connector()
		{
			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;
		}

		public Connector((IConnectable connectableA, IConnectable connectableB) connectables, Transform transform, GameData gameData) : this(transform, gameData)
		{
			Connectables = connectables;

			UpdateProxy();
		}

		public Connector(IConnectable initialConnectable, ConnectorSide side, Transform transform, GameData gameData) : this(transform, gameData)
		{
			Connectables = (side == ConnectorSide.A ? (initialConnectable, null) : (null, initialConnectable));

			UpdateProxy();
		}

		protected Connector(Transform transform, GameData gameData)
		{
			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

			Transform = transform;

			this.gameData = gameData;

			gameData.UI.SelectedEntityTypeChanged += UpdateProxyOnEntityTypeChanged;

			gameData.GameWorld.AddEntity(this);

			gameData.ReferenceHandler.RegisterReference(this);
		}

		private void UpdateProxyOnEntityTypeChanged(IEntityType _)
		{
			UpdateProxy();
		}

		public bool Destroyed { get; protected set; }

		private GameData gameData;

		public Transform Transform;

		public InformationProxy Proxy;

		public PhysicsObject PhysicsObject;

		public ConnectorSide? VacantSide
		{
			get
			{
				if (Connectables.connectableA is null)
				{
					return ConnectorSide.A;
				}
				else if (Connectables.connectableB is null)
				{
					return ConnectorSide.B;
				}
				else
				{
					return null;
				}
			}
		}

		public abstract IInformationProvider InformationProvider { get; }

		public bool Vacant => VacantSide is not null;

		public Guid SerializableReferenceGUID { get; private set; } = Guid.NewGuid();

		public (IConnectable? connectableA, IConnectable? connectableB) Connectables { get; protected set; }

		public IConnectable? Unconnected
		{
			get
			{
				if (!VacantSide.HasValue)
				{
					return null;
				}

				return (VacantSide == ConnectorSide.A ? Connectables.connectableB : Connectables.connectableA);
			}
		}

		public IConnectable? GetConnectableAtSide(ConnectorSide side)
		{
			return side == ConnectorSide.A ? Connectables.connectableA : Connectables.connectableB;
		}

		public IConnectable? GetOther(IConnectable connectable)
		{
			if (connectable == Connectables.connectableA)
			{
				return Connectables.connectableB;
			}
			else if (connectable == Connectables.connectableB)
			{
				return Connectables.connectableA;
			}
			else
			{
				throw new ArgumentException("Connectable is not present on connector.", nameof(connectable));
			}
		}

		public ConnectorSide GetConnectorSide(IConnectable connectable)
		{
			if (Connectables.connectableA == connectable)
			{
				return ConnectorSide.A;
			}
			else if (Connectables.connectableB == connectable)
			{
				return ConnectorSide.B;
			}
			else
			{
				throw new ArgumentException("Connectable is not present on connector.", nameof(connectable));
			}
		}

		public void Connect(IConnectable connectable)
		{
			if (!Vacant)
			{
				throw new Exception($"{nameof(Connector)} is not vacant.");
			}

			if (VacantSide == ConnectorSide.A)
			{
				Connectables = (connectable, Connectables.connectableB);
			}
			else if (VacantSide == ConnectorSide.B)
			{
				Connectables = (Connectables.connectableA, connectable);
			}

			PhysicsObject.Enabled = false;
			Proxy.Disable();
		}

		public void Disconnect(ConnectorSide side)
		{
			if (side == ConnectorSide.A)
			{
				Connectables = (null, Connectables.connectableB);
			}
			else if (side == ConnectorSide.B)
			{
				Connectables = (Connectables.connectableA, null);
			}

			PhysicsObject.Enabled = true;
			UpdateProxy();

			if (Connectables.connectableA is null && Connectables.connectableB is null)
			{
				Destroy();
			}
		}

		public void Disconnect(IConnectable connectable)
		{
			if (Connectables.connectableA == connectable)
			{
				Disconnect(ConnectorSide.A);
			}
			else if (Connectables.connectableB == connectable)
			{
				Disconnect(ConnectorSide.B);
			}
		}

		public void UpdateProxy()
		{
			if (!Vacant)
			{
				return;
			}

			if (Proxy is null)
			{
				Proxy = new(this);
			}

			if (Proxy.PhysicsObject is not null)
			{
				Proxy.PhysicsObject.Destroy();
			}

			var proxyPhysicsObject = new PhysicsObjectDescription<ConvexHull>(gameData.PhysicsWorld.GetConvexHullForMesh(gameData.UI.SelectedPipeType.Mesh), GameWorld.GenerateTransformForConnector(gameData.UI.SelectedPipeType.ConnectorPlacements[gameData.UI.ConnectorSelection], this, gameData.UI.RotationIndex * 45 * FixedDecimalLong8.DegreesToRadians), 0, true);

			Proxy.PhysicsObject = gameData.PhysicsWorld.AddPhysicsObject(proxyPhysicsObject, Proxy);

			Proxy.Enable();
		}

		public ConnectorSide? PopulatedSide
		{
			get
			{
				if (!VacantSide.HasValue)
				{
					return null;
				}

				return VacantSide.Value.Opposite();
			}
		}

		private void AddDebugObjects()
		{
			if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Connector)} debug objects"))
				return;

			MainGame.DebugRender.DrawOrientationMarks(PhysicsObject.Transform);

			MainGame.DebugRender.DrawCube(new Transform(PhysicsObject.Transform) { Scale = new((FixedDecimalInt4).4f, (FixedDecimalInt4).4f, (FixedDecimalInt4).25f) }, VacantSide is null ? RgbaFloat.Green : RgbaFloat.Cyan);
		}

		public virtual void SerializeState(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(Connector));
			{
				writer.SerializeReference(this);

				writer.Serialize(Transform);

				writer.Serialize(Connectables.connectableA is not null, "HasA");
				writer.Serialize(Connectables.connectableB is not null, "HasB");

				if (Connectables.connectableA is not null)
				{
					writer.SerializeReference(Connectables.connectableA, $"{nameof(Connectables.connectableA)}_GUID");
				}

				if (Connectables.connectableB is not null)
				{
					writer.SerializeReference(Connectables.connectableB, $"{nameof(Connectables.connectableB)}_GUID");
				}
			}
			writer.WriteEndElement();
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(Connector));
			{
				SerializableReferenceGUID = reader.ReadReferenceGUID();

				Transform = reader.DeserializeTransform();

				IConnectable a = null, b = null;
				bool hasA = reader.DeserializeBoolean("HasA");
				bool hasB = reader.DeserializeBoolean("HasB");

				if (hasA)
				{
					reader.DeserializeReference<IConnectable>(referenceHandler, (es) => a = (IConnectable)es, $"{nameof(Connectables.connectableA)}_GUID");
				}

				if (hasB)
				{
					reader.DeserializeReference<IConnectable>(referenceHandler, (es) => b = (IConnectable)es, $"{nameof(Connectables.connectableB)}_GUID");
				}

				serializationData.SerializationCompleteEvent += () =>
				{
					Connectables = (a, b);
				};

				this.gameData = serializationData.GameData;

				serializationData.GameData.GameWorld.AddEntity(this);
			}
			reader.ReadEndElement();
		}

		public virtual void Destroy()
		{
			if (Destroyed)
			{
				return;
			}

			Destroyed = true;

			PhysicsObject.Destroy();

			Proxy.PhysicsObject.Destroy();

			gameData.UI.SelectedEntityTypeChanged -= UpdateProxyOnEntityTypeChanged;

			MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

			gameData.GameWorld.RemoveEntity(this);

			gameData.ReferenceHandler.RemoveReference(this);
		}

		public abstract void Tick();

		public virtual void Interacted() { }
	}
}
