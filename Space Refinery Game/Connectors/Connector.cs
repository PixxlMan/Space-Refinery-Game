using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	public abstract class Connector : Entity, IDisposable
	{
		public Connector((IConnectable connectableA, IConnectable connectableB) connectables, Transform transform, GameWorld gameWorld, PhysicsWorld physicsWorld, UI ui) : this(transform, gameWorld, physicsWorld, ui)
		{
			Connectables = connectables;

			UpdateProxy();
		}

		public Connector(IConnectable initialConnectable, ConnectorSide side, Transform transform, GameWorld gameWorld, PhysicsWorld physicsWorld, UI ui) : this(transform, gameWorld, physicsWorld, ui)
		{
			Connectables = (side == ConnectorSide.A ? (initialConnectable, null) : (null, initialConnectable));

			VacantSide = side.Opposite();

			UpdateProxy();
		}

		protected Connector(Transform transform, GameWorld gameWorld, PhysicsWorld physicsWorld, UI ui)
		{
			MainGame.DebugRender.AddDebugObjects += AddDebugObjects;

			GameWorld = gameWorld;

			GameWorld.AddEntity(this);

			Transform = transform;

			PhysicsWorld = physicsWorld;

			UI = ui;

			UI.SelectedEntityTypeChanged += (_) =>
			{
				UpdateProxy();
			};
		}

		public UI UI;

		public GameWorld GameWorld;

		public PhysicsWorld PhysicsWorld;

		public Transform Transform;

		public ConnectorProxy Proxy;

		public PhysicsObject PhysicsObject;

		public ConnectorSide? VacantSide;

		public abstract IInformationProvider InformationProvider { get; }

		public bool Vacant => VacantSide is not null;

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

				if (Connectables.connectableB is not null)
				{
					VacantSide = null;
				}
			}
			else if (VacantSide == ConnectorSide.B)
			{
				Connectables = (Connectables.connectableA, connectable);

				if (Connectables.connectableA is not null)
				{
					VacantSide = null;
				}
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
			VacantSide = side;

			PhysicsObject.Enabled = true;
			UpdateProxy();

			if (Connectables.connectableA is null && Connectables.connectableB is null)
			{
				Dispose();
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

			var proxyPhysicsObject = new PhysicsObjectDescription<ConvexHull>(PhysicsWorld.GetConvexHullForMesh(UI.SelectedPipeType.Mesh), GameWorld.GenerateTransformForConnector(UI.SelectedPipeType.ConnectorPlacements[UI.ConnectorSelection], this, UI.RotationIndex * 45 * FixedDecimalLong8.DegreesToRadians), 0, true);

			Proxy.PhysicsObject = PhysicsWorld.AddPhysicsObject(proxyPhysicsObject, this);

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

		public void Dispose()
		{
			GameWorld.RemoveEntity(this);

			PhysicsObject.Destroy();

			Proxy.PhysicsObject.Destroy();

			MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;
		}
	}
}
