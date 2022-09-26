using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Space_Refinery_Game;

public partial class PhysicsWorld // https://github.com/bepu/bepuphysics2/blob/master/Demos/Demos/CollisionQueryDemo.cs
{
	/// <summary>
	/// Provides callbacks for filtering and data collection to the CollisionBatcher we'll be using to test query shapes against the detected environment.
	/// </summary>
	public struct BatcherCallbacks<T> : ICollisionCallbacks
		where T : Entity
	{
		public BatcherCallbacks(PhysicsWorld physicsWorld)
		{
			PhysicsWorld = physicsWorld;

			OverlappedObject = null;
		}

		public PhysicsObject? OverlappedObject;

		public PhysicsWorld PhysicsWorld;

		//These callbacks provide filtering and reporting for pairs being processed by the collision batcher.
		//"Pair id" refers to the identifier given to the pair when it was added to the batcher.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowCollisionTesting(int pairId, int childA, int childB)
		{
			//If you wanted to filter based on the children of an encountered nonconvex object, here would be the place to do it.
			//The pairId could be used to look up the involved objects and any metadata necessary for filtering.

			return true;

			//return PhysicsWorld.PhysicsObjectLookup[new BodyHandle(childB)].Entity is T;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnChildPairCompleted(int pairId, int childA, int childB, ref ConvexContactManifold manifold)
		{
			//If you need to do any processing on a child manifold before it goes back to a nonconvex processing pass, this is the place to do it.
			//Convex-convex pairs won't invoke this function at all.
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnPairCompleted<TManifold>(int pairId, ref TManifold manifold) where TManifold : unmanaged, IContactManifold<TManifold>
		{
			//This function hands off the completed manifold with all postprocessing (NonconvexReduction, MeshReduction, etc.) complete.
			//For the purposes of this demo, we're interested in boolean collision testing.
			//(This process was a little overkill for a pure boolean test, but there is no pure boolean path right now because the contact manifold generators turned out fast enough.
			//And if you find yourself wanting contact data, well, you've got it handy!)
			for (int i = 0; i < manifold.Count; ++i)
			{
				//This probably looks a bit odd, but it addresses a limitation of returning references to the struct 'this' instance.
				//(What we really want here is either the lifting of that restriction, or allowing interfaces to require a static member so that we could call the static function and pass the instance, 
				//instead of invoking the function on the instance AND passing the instance.)
				if (manifold.GetDepth(ref manifold, i) >= 0)
				{
					var bodyHandle = new BodyHandle(manifold.GetFeatureId(i)); // this is gravely incorrect

					if (PhysicsWorld.PhysicsObjectLookup.ContainsKey(bodyHandle))
					{
						var physicsObject = PhysicsWorld.PhysicsObjectLookup[bodyHandle];
						if (physicsObject.Entity is T)
						{
							OverlappedObject = physicsObject;
							break;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Called by the BroadPhase.GetOverlaps to collect all encountered collidables.
	/// </summary>
	struct BroadPhaseOverlapEnumerator : IBreakableForEach<CollidableReference>
	{
		public QuickList<CollidableReference> References;
		//The enumerator never gets stored into unmanaged memory, so it's safe to include a reference type instance.
		public BufferPool Pool;
		public bool LoopBody(CollidableReference reference)
		{
			References.Allocate(Pool) = reference;
			//If you wanted to do any top-level filtering, this would be a good spot for it.
			//The CollidableReference tells you whether it's a body or a static object and the associated handle. You can look up metadata with that.
			return true;
		}
	}

	void GetPoseAndShape(CollidableReference reference, out RigidPose pose, out TypedIndex shapeIndex)
	{
		//Collidables can be associated with either bodies or statics. We have to look in a different place depending on which it is.
		if (reference.Mobility == CollidableMobility.Static)
		{
			var collidable = simulation.Statics[reference.StaticHandle];
			pose = collidable.Pose;
			shapeIndex = collidable.Shape;
		}
		else
		{
			var bodyReference = simulation.Bodies[reference.BodyHandle];
			pose = bodyReference.Pose;
			shapeIndex = bodyReference.Collidable.Shape;
		}
	}

	/// <summary>
	/// Adds a shape query to the collision batcher.
	/// </summary>
	/// <param name="queryShapeType">Type of the shape to test.</param>
	/// <param name="queryShapeData">Shape data to test.</param>
	/// <param name="queryShapeSize">Size of the shape data in bytes.</param>
	/// <param name="queryBoundsMin">Minimum of the query shape's bounding box.</param>
	/// <param name="queryBoundsMax">Maximum of the query shape's bounding box.</param>
	/// <param name="queryPose">Pose of the query shape.</param>
	/// <param name="queryId">Id to use to refer to this query when the collision batcher finishes processing it.</param>
	/// <param name="batcher">Batcher to add the query's tests to.</param>
	public unsafe void AddQueryToBatch<T>(int queryShapeType, void* queryShapeData, int queryShapeSize, in Vector3 queryBoundsMin, in Vector3 queryBoundsMax, in RigidPose queryPose, int queryId, ref CollisionBatcher<BatcherCallbacks<T>> batcher)
		where T : Entity
	{
		var broadPhaseEnumerator = new BroadPhaseOverlapEnumerator { Pool = bufferPool, References = new QuickList<CollidableReference>(16, bufferPool) };
		simulation.BroadPhase.GetOverlaps(queryBoundsMin, queryBoundsMax, ref broadPhaseEnumerator);
		for (int overlapIndex = 0; overlapIndex < broadPhaseEnumerator.References.Count; ++overlapIndex)
		{
			GetPoseAndShape(broadPhaseEnumerator.References[overlapIndex], out var pose, out var shapeIndex);
			simulation.Shapes[shapeIndex.Type].GetShapeData(shapeIndex.Index, out var shapeData, out _);
			//In this path, we assume that the incoming shape data is ephemeral. The collision batcher may last longer than the data pointer.
			//To avoid undefined access, we cache the query data into the collision batcher and use a pointer to the cache instead.
			batcher.CacheShapeB(shapeIndex.Type, queryShapeType, queryShapeData, queryShapeSize, out var cachedQueryShapeData);
			batcher.AddDirectly(
				shapeIndex.Type, queryShapeType,
				shapeData, cachedQueryShapeData,
				//Because we're using this as a boolean query, we use a speculative margin of 0. Don't care about negative depths.
				queryPose.Position - pose.Position, queryPose.Orientation, pose.Orientation, 0, new PairContinuation(queryId));
		}
		broadPhaseEnumerator.References.Dispose(bufferPool);
	}

	/// <summary>
	/// Adds a shape query to the collision batcher.
	/// </summary>
	/// <typeparam name="TShape">Type of the query shape.</typeparam>
	/// <param name="shape">Shape to use in the query.</param>
	/// <param name="pose">Pose of the query shape.</param>
	/// <param name="queryId">Id to use to refer to this query when the collision batcher finishes processing it.</param>
	/// <param name="batcher">Batcher to add the query's tests to.</param>
	public unsafe void AddQueryToBatch<TShape, T>(TShape shape, in RigidPose pose, int queryId, ref CollisionBatcher<BatcherCallbacks<T>> batcher)
		where TShape : IConvexShape
		where T : Entity
	{
		var queryShapeData = Unsafe.AsPointer(ref shape);
		var queryShapeSize = Unsafe.SizeOf<TShape>();
		shape.ComputeBounds(pose.Orientation, out var boundingBoxMin, out var boundingBoxMax);
		boundingBoxMin += pose.Position;
		boundingBoxMax += pose.Position;
		AddQueryToBatch(shape.TypeId, queryShapeData, queryShapeSize, boundingBoxMin, boundingBoxMax, pose, queryId, ref batcher);
	}

	/// <summary>
	/// Adds a shape query to the collision batcher.
	/// </summary>
	/// <typeparam name="TShape">Type of the query shape.</typeparam>
	/// <param name="shape">Shape to use in the query.</param>
	/// <param name="pose">Pose of the query shape.</param>
	/// <param name="queryId">Id to use to refer to this query when the collision batcher finishes processing it.</param>
	/// <param name="batcher">Batcher to add the query's tests to.</param>
	public unsafe void AddQueryToBatch<T>(Shapes shapes, TypedIndex queryShapeIndex, in RigidPose queryPose, int queryId, ref CollisionBatcher<BatcherCallbacks<T>> batcher)
		where T : Entity
	{
		var shapeBatch = shapes[queryShapeIndex.Type];
		shapeBatch.ComputeBounds(queryShapeIndex.Index, queryPose, out var queryBoundsMin, out var queryBoundsMax);
		simulation.Shapes[queryShapeIndex.Type].GetShapeData(queryShapeIndex.Index, out var queryShapeData, out _);
		var broadPhaseEnumerator = new BroadPhaseOverlapEnumerator { Pool = bufferPool, References = new QuickList<CollidableReference>(16, bufferPool) };
		simulation.BroadPhase.GetOverlaps(queryBoundsMin, queryBoundsMax, ref broadPhaseEnumerator);
		for (int overlapIndex = 0; overlapIndex < broadPhaseEnumerator.References.Count; ++overlapIndex)
		{
			GetPoseAndShape(broadPhaseEnumerator.References[overlapIndex], out var pose, out var shapeIndex);
			//Since both involved shapes are from the simulation cache, we don't need to cache them ourselves.
			simulation.Shapes[shapeIndex.Type].GetShapeData(shapeIndex.Index, out var shapeData, out _);
			batcher.AddDirectly(
				shapeIndex.Type, queryShapeIndex.Type,
				shapeData, queryShapeData,
				//Because we're using this as a boolean query, we use a speculative margin of 0. Don't care about negative depths.
				queryPose.Position - pose.Position, queryPose.Orientation, pose.Orientation, 0, new PairContinuation(queryId));
		}
		broadPhaseEnumerator.References.Dispose(bufferPool);
	}

	struct Query
	{
		public Box Box;
		public RigidPose Pose;
	}

	public bool OverlapBox<T>(Transform transform, Vector3FixedDecimalInt4 scale, out PhysicsObject? physicsObject)
		where T : Entity
	{
		throw new NotImplementedException();

		Box box = new(scale.X.ToFloat(), scale.Y.ToFloat(), scale.Z.ToFloat());

		//simulation.Shapes.Add(box);

		CollisionBatcher<BatcherCallbacks<T>> collisionBatcher = new(bufferPool, simulation.Shapes, simulation.NarrowPhase.CollisionTaskRegistry, (float)Time.PhysicsInterval, new BatcherCallbacks<T>(this));

		RigidPose rigidPose = new(transform.Position.ToVector3(), transform.Rotation.ToQuaternion());

		AddQueryToBatch(box, rigidPose, 0, ref collisionBatcher);

		collisionBatcher.Flush();

		physicsObject = collisionBatcher.Callbacks.OverlappedObject;

		return collisionBatcher.Callbacks.OverlappedObject is not null;
	}
}