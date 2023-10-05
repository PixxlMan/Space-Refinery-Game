using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using FixedPrecision;

namespace Space_Refinery_Engine;

public partial class PhysicsWorld
{
	/*public struct OverlapPointHandler<T> : ISweepHitHandler
			where T : Entity
	{
		public PhysicsObject? PhysicsObject;

		private PhysicsWorld physicsWorld;

		public OverlapPointHandler(PhysicsWorld physicsWorld) : this()
		{
			this.physicsWorld = physicsWorld;
		}

		public bool AllowTest(CollidableReference collidable)
		{
			PhysicsObject physicsObject = physicsWorld.PhysicsObjectLookup[collidable.BodyHandle];

			return physicsObject.Entity is T && physicsObject.Enabled;
		}

		public bool AllowTest(CollidableReference collidable, int child)
		{
			PhysicsObject physicsObject = physicsWorld.PhysicsObjectLookup[collidable.BodyHandle];

			return physicsObject.Entity is T && physicsObject.Enabled;
		}

		public void OnHit(ref float maximumT, float t, in Vector3 hitLocation, in Vector3 hitNormal, CollidableReference collidable)
		{
			if (PhysicsObject is null)
				PhysicsObject = physicsWorld.PhysicsObjectLookup[collidable.BodyHandle];
		}

		public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
		{
			if (PhysicsObject is null)
				PhysicsObject = physicsWorld.PhysicsObjectLookup[collidable.BodyHandle];
		}
	}*/

	struct OverlapPointBroadPhaseOverlapEnumerator : IBreakableForEach<CollidableReference>
	{
		public QuickList<CollidableReference> References;

		public BufferPool BufferPool;

		public OverlapPointBroadPhaseOverlapEnumerator(BufferPool bufferPool)
		{
			References = new(1, bufferPool);
			BufferPool = bufferPool;
		}

		public bool LoopBody(CollidableReference reference)
		{
			References.Allocate(BufferPool) = reference;
			
			return true;
		}
	}

	public bool ApproxOverlapPoint<T>(Vector3FixedDecimalInt4 point, out PhysicsObject? physicsObject)
		where T : Entity
	{
		physicsObject = null;

		OverlapPointBroadPhaseOverlapEnumerator overlapEnumerator = new(bufferPool);

		simulation.BroadPhase.GetOverlaps(point.ToVector3(), point.ToVector3(), ref overlapEnumerator);

		foreach (var reference in overlapEnumerator.References)
		{
			var referencedPhysicsObject = PhysicsObjectLookup[reference.BodyHandle];
			if (referencedPhysicsObject.Entity is T)
			{
				physicsObject = referencedPhysicsObject;
			}
		}

		return physicsObject is not null;
	}
}