using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		}

		public Connector(IConnectable initialConnectable, ConnectorSide side, Transform transform, GameData gameData) : this((side == ConnectorSide.A ? (initialConnectable, null) : (null, initialConnectable)), transform, gameData)
		{

		}

		public Connector((IConnectable connectableA, IConnectable connectableB) connectables, Transform transform, GameData gameData)
		{
			Connectables = connectables;

			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

			Transform = transform;

			this.GameData = gameData;

			SetUp();
		}

		protected virtual void SetUp()
		{
			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

			GameData.UI.SelectedEntityTypeChanged += UpdateProxyOnEntityTypeChanged;

			UpdateProxyAndPhysicsObject();
		}

		private void UpdateProxyOnEntityTypeChanged(IEntityType _)
		{
			UpdateProxyAndPhysicsObject();
		}

		public bool Destroyed { get; protected set; }

		protected GameData GameData;

		public Transform Transform;

		public InformationProxy Proxy;

		public PhysicsObject PhysicsObject;

		protected object SyncRoot = new();

		public ConnectorSide? VacantSide
		{
			get
			{
				lock (SyncRoot)
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
		}

		public abstract IInformationProvider InformationProvider { get; }

		public bool Vacant => VacantSide is not null;

		public Guid SerializableReferenceGUID { get; private set; } = Guid.NewGuid();

		private (IConnectable connectableA, IConnectable connectableB) connectables;
		public (IConnectable? connectableA, IConnectable? connectableB) Connectables { get { lock(SyncRoot) return connectables; } protected set { lock (SyncRoot) { connectables = value; Debug.Assert(!(connectables.connectableA is null && connectables.connectableB is null), $"Both {nameof(Connectables)} cannot be null!"); } } }

		public IConnectable? Unconnected
		{
			get
			{
				lock (SyncRoot)
				{
					if (!VacantSide.HasValue)
					{
						return null;
					}

					return (VacantSide == ConnectorSide.A ? Connectables.connectableB : Connectables.connectableA);
				}
			}
		}

		public IConnectable? GetConnectableAtSide(ConnectorSide side)
		{
			lock (SyncRoot)
			{
				return side == ConnectorSide.A ? Connectables.connectableA : Connectables.connectableB;
			}
		}

		public IConnectable? GetOther(IConnectable connectable)
		{
			lock (SyncRoot)
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
		}

		public ConnectorSide GetConnectorSide(IConnectable connectable)
		{
			lock (SyncRoot)
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
		}

		public void Connect(IConnectable connectable)
		{
			lock (SyncRoot)
			{
				if (!Vacant)
				{
					throw new Exception($"{nameof(Connector)} is not vacant.");
				}

				Debug.Assert(!ReferenceEquals(GetConnectableAtSide(VacantSide.Value), connectable), "This connector has detected a self referential connection.");

				if (VacantSide == ConnectorSide.A)
				{
					Connectables = (connectable, Connectables.connectableB);
				}
				else if (VacantSide == ConnectorSide.B)
				{
					Connectables = (Connectables.connectableA, connectable);
				}

				UpdateProxyAndPhysicsObject();
			}
		}

		public void Disconnect(ConnectorSide side)
		{
			lock (SyncRoot)
			{
				if (side == ConnectorSide.A)
				{
					connectables = (null, connectables.connectableB);
				}
				else if (side == ConnectorSide.B)
				{
					connectables = (connectables.connectableA, null);
				}

				UpdateProxyAndPhysicsObject();

				if (connectables.connectableA is null && connectables.connectableB is null)
				{
					Destroy();
				}
			}
		}

		public void Disconnect(IConnectable connectable)
		{
			lock (SyncRoot)
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
		}

		private void CreatePhysicsObject()
		{
			lock (SyncRoot)
			{
				var physicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(.1f, .1f, .1f), Transform, 0, true);

				PhysicsObject = GameData.PhysicsWorld.AddPhysicsObject(physicsObjectDescription, this);

				PhysicsObject.Enabled = Vacant;
			}
		}

		public void UpdateProxyAndPhysicsObject()
		{
			lock (SyncRoot)
			{
				if (!Vacant) // If the connector is vacant the proxy should be disabled.
				{
					if (PhysicsObject is not null)
						PhysicsObject.Enabled = false;
					Proxy?.Disable();
					return;
				}

				if (Proxy is null) // Make sure there is a proxy.
				{
					Proxy = new(this);
				}

				if (Proxy.PhysicsObject is not null) // Always remove physics object to "make room" for new one that will be created shortly. Could theoretically repurpose old one for those mad performance gainz?
				{
					Proxy.PhysicsObject.Destroy();
				}

				var proxyPhysicsObject = new PhysicsObjectDescription<ConvexHull>(
					GameData.PhysicsWorld.GetConvexHullForMesh(GameData.UI.SelectedPipeType.Mesh),
					GameWorld.GenerateTransformForConnector(
						GameData.UI.SelectedPipeType.ConnectorPlacements[GameData.UI.ConnectorSelection],
						this, GameData.UI.RotationIndex * 45 * FixedDecimalLong8.DegreesToRadians),
					0,
					true);

				Proxy.PhysicsObject = GameData.PhysicsWorld.AddPhysicsObject(proxyPhysicsObject, Proxy);

				if (Vacant)
				{
					Proxy.Enable();
				}
				else
				{
					Proxy.Disable();
				}

				if (PhysicsObject is null)
				{
					CreatePhysicsObject();
				}
				else if (PhysicsObject.Enabled && !PhysicsObject.Destroyed)
				{
					PhysicsObject.Transform = Transform;
				}

				PhysicsObject.Enabled = Vacant;
			}
		}

		public ConnectorSide? PopulatedSide
		{
			get
			{
				lock (SyncRoot)
				{
					if (!VacantSide.HasValue)
					{
						return null;
					}

					return VacantSide.Value.Opposite();
				}
			}
		}

		private void AddDebugObjects()
		{
			lock (SyncRoot)
			{
				if (Destroyed)
					return;

				if (!MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>($"{nameof(Connector)} debug objects"))
					return;

				MainGame.DebugRender.DrawOrientationMarks(Transform);

				MainGame.DebugRender.DrawCube(new Transform(PhysicsObject.Transform), VacantSide is null ? RgbaFloat.Green : RgbaFloat.Cyan, new((FixedDecimalInt4).4f, (FixedDecimalInt4).4f, (FixedDecimalInt4).25f));
			}
		}

		public virtual void SerializeState(XmlWriter writer)
		{
			lock (SyncRoot)
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
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			lock (SyncRoot)
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

					this.GameData = serializationData.GameData;

					serializationData.DeserializationCompleteEvent += () =>
					{
						if (a != null)
						{
							Connect(a);
						}

						if (b != null)
						{
							Connect(b);
						}

						SetUp();
					};

					this.GameData = serializationData.GameData;

					serializationData.GameData.GameWorld.AddEntity(this);
				}
				reader.ReadEndElement();
			}
		}

		public virtual void Destroy()
		{
			lock (SyncRoot)
			{
				if (Destroyed)
				{
					return;
				}

				Destroyed = true;

				PhysicsObject.Destroy();

				Proxy?.PhysicsObject.Destroy();

				GameData.UI.SelectedEntityTypeChanged -= UpdateProxyOnEntityTypeChanged;

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

				GameData.GameWorld.RemoveEntity(this);

				GameData.ReferenceHandler.RemoveReference(this);
			}
		}

		public abstract void Tick();

		public virtual void Interacted() { }
	}
}
