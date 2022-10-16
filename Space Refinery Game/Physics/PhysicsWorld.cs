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
	public sealed partial class PhysicsWorld
	{
		public Dictionary<BodyHandle, PhysicsObject> PhysicsObjectLookup = new();

		public object SyncRoot = new();

		private Dictionary<FXRenderer.Mesh, ConvexHull> convexHulls = new();

		private BufferPool bufferPool;

		private Simulation simulation;

		private IThreadDispatcher threadDispatcher;

		public event Action<FixedDecimalLong8> CollectPhysicsPerformanceData;

		public void SetUp()
		{
			lock (SyncRoot)
			{
				//The buffer pool is a source of raw memory blobs for the engine to use.
				bufferPool = new BufferPool();

				//The following sets up a simulation with the callbacks defined above, and tells it to use 8 velocity iterations per substep and only one substep per solve.
				//It uses the default SubsteppingTimestepper. You could use a custom ITimestepper implementation to customize when stages run relative to each other, or to insert more callbacks.         
				simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new SolveDescription(8, 1));

				//Any IThreadDispatcher implementation can be used for multithreading. Here, we use the BepuUtilities.ThreadDispatcher implementation.
				threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
			}
		}

		public void Run()
		{
			Thread thread = new Thread(new ThreadStart(() =>
			{
				Stopwatch stopwatch = new();
				stopwatch.Start();

				FixedDecimalLong8 timeLastUpdate = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>();
				FixedDecimalLong8 time;
				FixedDecimalLong8 deltaTime;
				while (true)
				{
					time = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>();

					deltaTime = time - timeLastUpdate;

					timeLastUpdate = time;

					CollectPhysicsPerformanceData?.Invoke(deltaTime);				

					lock (SyncRoot)
					{
						simulation.Timestep(Time.PhysicsInterval.ToFloat(), threadDispatcher);
					}

					if (deltaTime < Time.PhysicsInterval)
					{
						while (deltaTime < Time.PhysicsInterval)
						{
							deltaTime = stopwatch.Elapsed.TotalSeconds.ToFixed<FixedDecimalLong8>() - timeLastUpdate;

							if (Time.TickInterval > (FixedDecimalLong8)0.002 && Time.PhysicsInterval - deltaTime > (FixedDecimalLong8)0.002)
							{
								Thread.Sleep(1);
							}
							else
							{
								Thread.SpinWait(4);
							}
						}
					}
				}
			}))
			{ Name = "Physics Update Thread" };

			thread.Start();
		}

		public PhysicsObject AddPhysicsObject<TShape>(PhysicsObjectDescription<TShape> physicsObjectDescription, Entity entity) where TShape : unmanaged, IConvexShape
		{
			lock (SyncRoot)
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

				PhysicsObject physicsObject = new PhysicsObject(this, bodyHandle, entity);

				PhysicsObjectLookup.Add(bodyHandle, physicsObject);

				return physicsObject;
			}
		}

		private ConvexHull AddConvexHullForMesh(FXRenderer.Mesh mesh)
		{
			lock (SyncRoot)
			{
				ConvexHull convexHull = new(mesh.Points.AsSpan(), bufferPool, out _);

				convexHulls.Add(mesh, convexHull);

				return convexHull;
			}
		}

		public ConvexHull GetConvexHullForMesh(FXRenderer.Mesh mesh)
		{
			lock (SyncRoot)
			{
				if (convexHulls.ContainsKey(mesh))
				{
					return convexHulls[mesh];				
				}
				else
				{
					return AddConvexHullForMesh(mesh);
				}
			}
		}

		public void DestroyPhysicsObject(PhysicsObject physicsObject)
		{
			lock (SyncRoot)
			{
				PhysicsObjectLookup.Remove(physicsObject.BodyHandle);

				simulation.Bodies.Remove(physicsObject.BodyHandle);
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

				var physicsObject = physicsWorld.PhysicsObjectLookup[collidable.BodyHandle];

				if (physicsObject.Entity is T && physicsObject.Enabled)
				{
					PhysicsObject = physicsWorld.PhysicsObjectLookup[collidable.BodyHandle];
				}
			}
		}

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

				simulation.RayCast(start.ToVector3(), direction.ToVector3(), maxDistance.ToFloat(), ref raycastHitHandler);

				if (raycastHitHandler.PhysicsObject is null)
				{
					return null;
				}

				return raycastHitHandler.PhysicsObject;
			}
		}

		public Transform GetTransform(BodyHandle bodyHandle)
		{
			lock (SyncRoot)
			{
				RigidPose pose = simulation.Bodies[bodyHandle].Pose;

				return new(pose.Position.ToFixed<Vector3FixedDecimalInt4>(), pose.Orientation.ToFixed<QuaternionFixedDecimalInt4>());
			}
		}
	}
}