using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using System.Diagnostics;
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
			Transform = transform;

			GameData = gameData;

			this.connectables = connectables;

			VacancyStateChanged();

			SetUp();
		}

		/// <summary>
		/// This method exists mainly for the purposes of deserialization.
		/// </summary>
		protected virtual void SetUp()
		{
			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

			GameData.UI.SelectedEntityTypeChanged += (_) => UpdateProxyOnSelectedEntityAffected();

			GameData.UI.SelectedEntityRotated += (_) => UpdateProxyOnSelectedEntityAffected();

			GameData.UI.SelectedEntityConnectorChanged += (_) => UpdateProxyOnSelectedEntityAffected();
		}

		public bool Destroyed { get; protected set; }

		protected GameData GameData;

		public Transform Transform;

		public InformationProxy? Proxy;

		public PhysicsObject? PhysicsObject;

		protected object SyncRoot = new();

		private void UpdateProxyOnSelectedEntityAffected()
		{
			InvalidateAndUpdateProxy();
		}

		/// <summary>
		/// Updates the proxy object to match the transformation and physical shape of the currently selected entity.
		/// </summary>
		private void InvalidateAndUpdateProxy()
		{
			if (GameData.UI.SelectedPipeType is null)
			{
				Proxy.Disable();
			}
			else
			{
				SetProxyState(Vacant);

				var shape = GameData.PhysicsWorld.GetConvexHullForMesh(GameData.UI.SelectedPipeType.Mesh);

				var transform = GenerateTransformForConnector(
							GameData.UI.SelectedPipeType.ConnectorPlacements[GameData.UI.ConnectorSelection],
							this, GameData.UI.RotationSnapped);

				Proxy.SetPhysicsObjectState(transform, shape, GameData.PhysicsWorld);
			}
		}

		/// <summary>
		/// Updates relevant state when the state of vacancy is changed. Updates the proxy (disables it if not vacant) and physicsobject (disables it if not vacant).
		/// </summary>
		private void VacancyStateChanged()
		{
			if (Vacant)
			{
				SetProxyState(true);
				SetPhysicsObjectState(true);
			}
			else
			{
				SetProxyState(false);
				SetPhysicsObjectState(false);
			}
		}

		private void SetProxyState(bool enabled)
		{
			lock (SyncRoot)
			{
				if (enabled)
				{
					if (Proxy is null)
					{
						CreateProxy();
					}

					Proxy.Enable();
				}
				else
				{
					if (Proxy is not null)
					{
						Proxy.Disable();
					}
				}
			}
		}

		private void CreateProxy()
		{
			lock (SyncRoot)
			{
				Proxy = new(this);

				// Make sure that Proxy gets updated to have the correct collider etc according to object state.
				InvalidateAndUpdateProxy();
			}
		}

		private void SetPhysicsObjectState(bool enabled)
		{
			lock (SyncRoot)
			{
				if (enabled)
				{
					if (PhysicsObject is null)
					{
						CreatePhysicsObject();
					}

					PhysicsObject.Enabled = true;
				}
				else
				{
					if (PhysicsObject is not null)
					{
						PhysicsObject.Enabled = false;
					}
				}
			}
		}

		private void CreatePhysicsObject()
		{
			lock (SyncRoot)
			{
				var physicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(.1f, .1f, .1f), Transform, 0, true);

				PhysicsObject = GameData.PhysicsWorld.AddPhysicsObject(physicsObjectDescription, this);
			}
		}

		public ConnectorSide? VacantSide
		{
			get
			{
				lock (SyncRoot)
				{
					if (connectables.connectableA is null)
					{
						return ConnectorSide.A;
					}
					else if (connectables.connectableB is null)
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

		public bool Vacant => VacantSide is not null;

		public abstract IInformationProvider InformationProvider { get; }

		public SerializableReference SerializableReference { get; private set; } = Guid.NewGuid();

		private (IConnectable connectableA, IConnectable connectableB) connectables;
		/// <summary>
		/// Automatically updates things dependent on the Vacancy status of the connector.
		/// </summary>
		public (IConnectable? connectableA, IConnectable? connectableB) Connectables
		{
			get
			{
				lock (SyncRoot) return connectables;
			}
			protected set
			{
				lock (SyncRoot)
				{
					connectables = value;

					Debug.Assert(!(connectables.connectableA is null && connectables.connectableB is null), $"Both {nameof(Connectables)} cannot be null!");
				}
			}
		}

		public IConnectable? Unconnected
		{
			get
			{
				lock (SyncRoot)
				{
					if (!Vacant)
					{
						return null;
					}

					return (VacantSide == ConnectorSide.A ? connectables.connectableB : connectables.connectableA);
				}
			}
		}

		public IConnectable? GetConnectableAtSide(ConnectorSide side)
		{
			lock (SyncRoot)
			{
				return side == ConnectorSide.A ? connectables.connectableA : connectables.connectableB;
			}
		}

		public IConnectable? GetOther(IConnectable connectable)
		{
			lock (SyncRoot)
			{
				if (connectable == connectables.connectableA)
				{
					return connectables.connectableB;
				}
				else if (connectable == connectables.connectableB)
				{
					return connectables.connectableA;
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
				if (connectables.connectableA == connectable)
				{
					return ConnectorSide.A;
				}
				else if (connectables.connectableB == connectable)
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
					connectables = (connectable, connectables.connectableB);
				}
				else if (VacantSide == ConnectorSide.B)
				{
					connectables = (connectables.connectableA, connectable);
				}

				VacancyStateChanged();
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

				if (connectables.connectableA is null && connectables.connectableB is null)
				{
					Destroy();
				}

				VacancyStateChanged();
			}
		}

		public void Disconnect(IConnectable connectable)
		{
			lock (SyncRoot)
			{
				if (connectables.connectableA == connectable)
				{
					Disconnect(ConnectorSide.A);
				}
				else if (connectables.connectableB == connectable)
				{
					Disconnect(ConnectorSide.B);
				}
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

					writer.Serialize(connectables.connectableA is not null, "HasA");
					writer.Serialize(connectables.connectableB is not null, "HasB");

					if (connectables.connectableA is not null)
					{
						writer.SerializeReference(connectables.connectableA, $"{nameof(connectables.connectableA)}_GUID");
					}

					if (connectables.connectableB is not null)
					{
						writer.SerializeReference(connectables.connectableB, $"{nameof(connectables.connectableB)}_GUID");
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
					SerializableReference = reader.ReadReference();

					Transform = reader.DeserializeTransform();

					IConnectable a = null, b = null;
					bool hasA = reader.DeserializeBoolean("HasA");
					bool hasB = reader.DeserializeBoolean("HasB");

					if (hasA)
					{
						reader.DeserializeReference<IConnectable>(referenceHandler, (es) => a = (IConnectable)es, $"{nameof(connectables.connectableA)}_GUID");
					}

					if (hasB)
					{
						reader.DeserializeReference<IConnectable>(referenceHandler, (es) => b = (IConnectable)es, $"{nameof(connectables.connectableB)}_GUID");
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

				Proxy.Destroy();

				GameData.UI.SelectedEntityTypeChanged -= (_) => UpdateProxyOnSelectedEntityAffected();
				GameData.UI.SelectedEntityRotated -= (_) => UpdateProxyOnSelectedEntityAffected();
				GameData.UI.SelectedEntityConnectorChanged -= (_) => UpdateProxyOnSelectedEntityAffected();

				MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

				GameData.GameWorld.RemoveEntity(this);

				GameData.ReferenceHandler.RemoveReference(this);
			}
		}

		public abstract void Tick();

		public virtual void Interacted() { }

		public static Transform GenerateTransformForConnector(PositionAndDirection chosenConnectorTransform, Connector connector, FixedDecimalLong8 rotation)
		{
			QuaternionFixedDecimalInt4 connectorRotation = /*connector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(connector.Transform.Rotation) :*/ connector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			Transform pipeConnectorTransform = new Transform(connector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Direction : chosenConnectorTransform.Direction;

			Vector3FixedDecimalInt4 position = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Position : chosenConnectorTransform.Position;

			Transform transform =
				new(
					connector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransform.LocalUnitZ : pipeConnectorTransform.LocalUnitZ, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransform.LocalUnitY : pipeConnectorTransform.LocalUnitY))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransform.LocalUnitZ, -pipeConnectorTransform.LocalUnitY), QuaternionFixedDecimalInt4.CreateFromAxisAngle(direction, (FixedDecimalInt4)rotation)))
				);

			transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

			return transform;
		}
	}
}
