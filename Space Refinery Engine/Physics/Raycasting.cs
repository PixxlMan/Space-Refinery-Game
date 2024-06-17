using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using FixedPrecision;
using System.Numerics;

namespace Space_Refinery_Engine
{
	public sealed partial class PhysicsWorld
	{
		public PhysicsObject? Raycast(Vector3FixedDecimalInt4 start, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 maxDistance)
		{
			lock (SyncRoot)
			{
				return Raycast<Entity>(start, direction, maxDistance);
			}
		}

		public PhysicsObject? Raycast<T>(Vector3FixedDecimalInt4 start, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 maxDistance)
			where T : Entity
		{
			lock (SyncRoot)
			{
				var raycastHitHandler = new RaycastHitHandler<T>(this);

				Simulation.RayCast(start.ToVector3(), direction.ToVector3(), maxDistance.ToFloat(), ref raycastHitHandler);

				if (raycastHitHandler.PhysicsObject is null)
				{
					return null;
				}

				return raycastHitHandler.PhysicsObject;
			}
		}

		struct RaycastHitHandler<T> : IRayHitHandler
			where T : Entity
		{
			public PhysicsObject? PhysicsObject;

			private PhysicsWorld physicsWorld;

			public RaycastHitHandler(PhysicsWorld physicsWorld) : this()
			{
				this.physicsWorld = physicsWorld;
			}

			public bool AllowTest(CollidableReference collidable)
			{
				return true;
			}

			public bool AllowTest(CollidableReference collidable, int childIndex)
			{
				return true;
			}

			public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
			{
				if (PhysicsObject is not null)
				{
					return;
				}

				var physicsObject = physicsWorld.BodyHandleToPhysicsObject[collidable.BodyHandle];

				if (physicsObject.Entity is T && physicsObject.Enabled && physicsObject.RecievesRaycasts)
				{
					PhysicsObject = physicsWorld.BodyHandleToPhysicsObject[collidable.BodyHandle];
				}
			}
		}
	}
}