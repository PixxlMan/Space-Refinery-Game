using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Veldrid.Utilities;

namespace Space_Refinery_Game
{
	public partial class PhysicsWorld
	{
		public Dictionary<BodyHandle, PhysicsObject> PhysicsObjectLookup = new();

		private BufferPool bufferPool;

		private Simulation simulation;

		private IThreadDispatcher threadDispatcher;

		public void SetUp()
		{
			//The buffer pool is a source of raw memory blobs for the engine to use.
			bufferPool = new BufferPool();

			//The following sets up a simulation with the callbacks defined above, and tells it to use 8 velocity iterations per substep and only one substep per solve.
			//It uses the default SubsteppingTimestepper. You could use a custom ITimestepper implementation to customize when stages run relative to each other, or to insert more callbacks.         
			simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new SolveDescription(8, 1));

			//Any IThreadDispatcher implementation can be used for multithreading. Here, we use the BepuUtilities.ThreadDispatcher implementation.
			threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
		}

		public void Run()
		{
			Thread thread = new Thread(new ParameterizedThreadStart((_) =>
			{
				while (true)
				{
					Thread.Sleep(16);

					simulation.Timestep(0.016f, threadDispatcher);
				}
			}));

			thread.Start();
		}

		public PhysicsObject AddPhysicsObject<TShape>(PhysicsObjectDescription<TShape> physicsObjectDescription) where TShape : unmanaged, IConvexShape
		{
			var inertia = physicsObjectDescription.Shape.ComputeInertia(physicsObjectDescription.Mass.ToFloat());

			BodyHandle bodyHandle;

			if (!physicsObjectDescription.Kinematic)
			{
				bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(physicsObjectDescription.InitialTransform.Position.ToVector3(), physicsObjectDescription.InitialTransform.Rotation.ToQuaternion()), inertia, simulation.Shapes.Add(physicsObjectDescription.Shape), 0.01f));
			}
			else
			{
				bodyHandle = simulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(physicsObjectDescription.InitialTransform.Position.ToVector3(), physicsObjectDescription.InitialTransform.Rotation.ToQuaternion()), simulation.Shapes.Add(physicsObjectDescription.Shape), 0.01f));
			}

			PhysicsObject physicsObject = new PhysicsObject(this, bodyHandle);

			PhysicsObjectLookup.Add(bodyHandle, physicsObject);

			return physicsObject;
		}

		struct RaycastHitHandler : IRayHitHandler
		{
			public BodyHandle? BodyHandle;

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
				BodyHandle = collidable.BodyHandle;
			}
		}

		public PhysicsObject Raycast(Vector3FixedDecimalInt4 start, Vector3FixedDecimalInt4 direction, FixedDecimalInt4 maxDistance)
		{
			var raycastHitHandler = new RaycastHitHandler();

			simulation.RayCast(start.ToVector3(), direction.ToVector3(), maxDistance.ToFloat(), ref raycastHitHandler);

			if (raycastHitHandler.BodyHandle is null)
			{
				return null;
			}

			return PhysicsObjectLookup[raycastHitHandler.BodyHandle.Value];
		}

		public Transform GetTransform(BodyHandle bodyHandle)
		{
			RigidPose pose = simulation.Bodies[bodyHandle].Pose;

			return new(pose.Position.ToFixed<Vector3FixedDecimalInt4>(), pose.Orientation.ToFixed<QuaternionFixedDecimalInt4>());
		}
	}
}