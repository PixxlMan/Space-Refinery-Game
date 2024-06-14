using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Diagnostics;
using System.Numerics;

namespace Space_Refinery_Engine;

public sealed partial class PhysicsWorld
{
	public Dictionary<BodyHandle, PhysicsObject> PhysicsObjectLookup = new();

	public object SyncRoot = new();

	private Dictionary<Space_Refinery_Game_Renderer.Mesh, ConvexHull> convexHulls = new();

	private BufferPool bufferPool;

	private Simulation simulation;

	private IThreadDispatcher threadDispatcher;

	public event Action<IntervalUnit>? CollectPhysicsPerformanceData;

	public Vector3 Gravity = new(0, -9.82f, 0);

	public float LinearDamping = 0.04f;

	public float AngularDamping = 0.01f;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	public void SetUp()
	{
		lock (SyncRoot)
		{
			//The buffer pool is a source of raw memory blobs for the engine to use.
			bufferPool = new BufferPool();

			//The following sets up a simulation with the callbacks defined above, and tells it to use 8 velocity iterations per substep and only one substep per solve.
			//It uses the default SubsteppingTimestepper. You could use a custom ITimestepper implementation to customize when stages run relative to each other, or to insert more callbacks.         
			simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(Gravity, LinearDamping, AngularDamping), new SolveDescription(8, 1));

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

			TimeUnit timeLastUpdate = stopwatch.Elapsed.TotalSeconds;
			TimeUnit time;
			IntervalUnit deltaTime;
			while (true)
			{
				time = stopwatch.Elapsed.TotalSeconds;

				deltaTime = time - timeLastUpdate;

				CollectPhysicsPerformanceData?.Invoke(deltaTime);				

				lock (SyncRoot)
				{
					simulation.Timestep((float)(DecimalNumber)Time.PhysicsInterval, threadDispatcher);
				}

				Time.ResponseSpinner(time, ref responseSpinner);

				Time.WaitIntervalLimit(Time.PhysicsInterval, time, stopwatch, out var timeOfContinuation);

				timeLastUpdate = timeOfContinuation;
			}
		}))
		{ Name = "Physics Update Thread" };

		thread.Start();
	}

	public bool HasHandle(BodyHandle bodyHandle)
	{
		return PhysicsObjectLookup.ContainsKey(bodyHandle);
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

	public void DestroyPhysicsObject(PhysicsObject physicsObject)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(physicsObject.BodyHandle);

			PhysicsObjectLookup.Remove(physicsObject.BodyHandle);

			simulation.Bodies.Remove(physicsObject.BodyHandle);
		}
	}

	/// <summary>
	/// Equivalent to calling DestroyPhysicsObject for all objects, but much faster.
	/// </summary>
	public void Reset()
	{
		lock (SyncRoot)
		{
			PhysicsObjectLookup.Clear();
			convexHulls.Clear(); // When the simulation is cleared, this will be outdated, so it must too be cleared.
			simulation.Clear();
			simulation.Bodies.Clear();
		}
	}

	private ConvexHull AddConvexHullForMesh(Space_Refinery_Game_Renderer.Mesh mesh)
	{
		lock (SyncRoot)
		{
			ConvexHull convexHull = new(mesh.Points.AsSpan(), bufferPool, out _);

			convexHulls.Add(mesh, convexHull);

			return convexHull;
		}
	}

	public ConvexHull GetConvexHullForMesh(Space_Refinery_Game_Renderer.Mesh mesh)
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

	public void AddImpulse(PhysicsObject physicsObject, Vector3FixedDecimalInt4 impulse)
	{
		lock (SyncRoot)
		{
			Vector3 floatImpulse = impulse.ToVector3();
			Vector3 offset = physicsObject.Transform.Position.ToVector3();
			new BodyReference(physicsObject.BodyHandle, simulation.Bodies).ApplyImpulse(in floatImpulse, in offset);
		}
	}

	public void SetShape(PhysicsObject physicsObject, ConvexHull shape)
	{ // OPTIMIZE: remove old shapes and don't always add the new ones if identical ones are already used (pass TypedIndex instead of TShape to the AddPhysicsObject method to accomodate more easily sharing the same shape between pipes of the same type)
		lock (SyncRoot)
		{
			PhysicsObjectNotDestroyedSafetyCheck(physicsObject);
			HandleExistsSafetyCheck(physicsObject.BodyHandle);

			// Something here causes bepu physics to get unstable.
			var oldShape = simulation.Bodies[physicsObject.BodyHandle].Collidable.Shape;
			simulation.Bodies[physicsObject.BodyHandle].SetShape(simulation.Shapes.Add(shape));
			//simulation.Shapes.RecursivelyRemoveAndDispose(oldShape, bufferPool);
			//simulation.Shapes.RemoveAndDispose(oldShape, bufferPool);
			//simulation.Shapes.Remove(oldShape); // Least broken option
		}
	}

	public Transform GetTransform(BodyHandle bodyHandle)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(bodyHandle);

			RigidPose pose = simulation.Bodies[bodyHandle].Pose;

			return new(pose.Position.ToFixed<Vector3FixedDecimalInt4>(), pose.Orientation.ToFixed<QuaternionFixedDecimalInt4>());
		}
	}

	public void SetTransform(BodyHandle bodyHandle, Transform transform)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(bodyHandle);

			RigidPose pose = simulation.Bodies[bodyHandle].Pose;

			pose.Position = transform.Position.ToVector3();

			pose.Orientation = transform.Rotation.ToQuaternion();
		}
	}

	private void HandleExistsSafetyCheck(BodyHandle bodyHandle)
	{
		if (!PhysicsObjectLookup.ContainsKey(bodyHandle))
		{
			throw new ArgumentException($"{nameof(BodyHandle)} is either invalid or the corresponding object is destroyed!", nameof(bodyHandle));
		}
	}

	private void PhysicsObjectNotDestroyedSafetyCheck(PhysicsObject physicsObject)
	{
		if (physicsObject.Destroyed)
		{
			throw new ArgumentException($"{nameof(PhysicsObject)} is destroyed!", nameof(physicsObject));
		}
	}
}