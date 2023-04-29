using BepuPhysics.Collidables;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public sealed class InformationProxy : Entity
	{
		public Entity ProxiedEntity { get; }

		private PhysicsObject? physicsObject;

		private bool enabled = false;

		public InformationProxy(Connector connector)
		{
			ProxiedEntity = connector;
		}

		public void Enable()
		{
			enabled = true;

			if (physicsObject is not null)
			{
				physicsObject.Enabled = true;
			}
		}

		public void Disable()
		{
			enabled = false;

			if (physicsObject is not null)
			{
				physicsObject.Enabled = false;
			}
		}

		public void SetPhysicsObjectState(Transform transform, ConvexHull shape, PhysicsWorld physicsWorld)
		{
			if (physicsObject is null)
			{
				var proxyPhysicsObject = new PhysicsObjectDescription<ConvexHull>(shape, transform, 0, true);

				physicsObject = physicsWorld.AddPhysicsObject(proxyPhysicsObject, this);

				physicsObject.Enabled = enabled;
			}
			else
			{
				physicsObject.World.ChangeShape(physicsObject, shape);
			}
		}

		public void Tick() => throw new NotSupportedException();

		public void Interacted() => throw new NotSupportedException();

		public void Destroy()
		{
			physicsObject?.Destroy();
		}

		public IInformationProvider InformationProvider => ((Entity)ProxiedEntity).InformationProvider;
	}
}
