using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;

namespace Space_Refinery_Engine;

public sealed partial class PhysicsWorld
{
	private Dictionary<BodyHandle, PhysicsObject> bodyHandleToPhysicsObject = new();
	public ReadOnlyDictionary<BodyHandle, PhysicsObject> BodyHandleToPhysicsObject { get; private set; }

	public object SyncRoot = new();

	public BufferPool BufferPool { get; private set; }

	public Simulation Simulation { get; private set; }

	public IThreadDispatcher ThreadDispatcher { get; private set; }

	public event Action<IntervalUnit>? CollectPhysicsPerformanceData;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	private Dictionary<Space_Refinery_Game_Renderer.Mesh, ConvexHull> convexHulls = new();

	public PhysicsWorld()
	{
		BodyHandleToPhysicsObject = bodyHandleToPhysicsObject.AsReadOnly();
	}

	public void SetUp(Simulation simulation, BufferPool bufferPool, IThreadDispatcher threadDispatcher)
	{
		lock (SyncRoot)
		{
			Simulation = simulation;
			BufferPool = bufferPool;
			ThreadDispatcher = threadDispatcher;
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
						OnPhysicsUpdate?.Invoke(Time.PhysicsInterval);
					Simulation.Timestep((float)(DecimalNumber)Time.PhysicsInterval, ThreadDispatcher);
				}

				Time.ResponseSpinner(time, ref responseSpinner);

				Time.WaitIntervalLimit(Time.PhysicsInterval, time, stopwatch, out var timeOfContinuation);

				timeLastUpdate = timeOfContinuation;
			}
		}))
		{ Name = "Physics Update Thread" };

		thread.Start();
	}

	public Action<IntervalUnit> OnPhysicsUpdate;

	public bool HasHandle(BodyHandle bodyHandle)
	{
		return BodyHandleToPhysicsObject.ContainsKey(bodyHandle);
	}

	public PhysicsObject AddPhysicsObject<TShape>(PhysicsObjectDescription<TShape> physicsObjectDescription, Entity entity) where TShape : unmanaged, IConvexShape
	{
		lock (SyncRoot)
		{
			var inertia = physicsObjectDescription.Shape.ComputeInertia(physicsObjectDescription.Mass.ToFloat());

			BodyHandle bodyHandle;

			if (!physicsObjectDescription.Kinematic)
			{
				bodyHandle = Simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(physicsObjectDescription.InitialTransform.Position.ToVector3(), physicsObjectDescription.InitialTransform.Rotation.ToQuaternion()), inertia, Simulation.Shapes.Add(physicsObjectDescription.Shape), 0.01f));
			}
			else
			{
				bodyHandle = Simulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(physicsObjectDescription.InitialTransform.Position.ToVector3(), physicsObjectDescription.InitialTransform.Rotation.ToQuaternion()), Simulation.Shapes.Add(physicsObjectDescription.Shape), 0.01f));
			}

			PhysicsObject physicsObject = new PhysicsObject(this, bodyHandle, entity);

			bodyHandleToPhysicsObject.Add(bodyHandle, physicsObject);

			return physicsObject;
		}
	}

	public PhysicsObject AddPhysicsObject(BodyDescription bodyDescription, Entity entity)
	{
		lock (SyncRoot)
		{
			BodyHandle bodyHandle;

			bodyHandle = Simulation.Bodies.Add(bodyDescription);

			PhysicsObject physicsObject = new PhysicsObject(this, bodyHandle, entity);

			bodyHandleToPhysicsObject.Add(bodyHandle, physicsObject);

			return physicsObject;
		}
	}

	public void DestroyPhysicsObject(PhysicsObject physicsObject)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(physicsObject.BodyHandle);

			bodyHandleToPhysicsObject.Remove(physicsObject.BodyHandle);

			Simulation.Bodies.Remove(physicsObject.BodyHandle);
		}
	}

	/// <summary>
	/// Equivalent to calling DestroyPhysicsObject for all objects, but much faster.
	/// </summary>
	public void Reset()
	{
		lock (SyncRoot)
		{
			bodyHandleToPhysicsObject.Clear();
			convexHulls.Clear(); // When the simulation is cleared, this will be outdated, so it must too be cleared.
			Simulation.Clear();
			Simulation.Bodies.Clear();
		}
	}

	private ConvexHull AddConvexHullForMesh(Space_Refinery_Game_Renderer.Mesh mesh)
	{
		lock (SyncRoot)
		{
			ConvexHull convexHull = new(mesh.Points.AsSpan(), BufferPool, out _);

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
			new BodyReference(physicsObject.BodyHandle, Simulation.Bodies).ApplyImpulse(in floatImpulse, in offset);
		}
	}

	public void SetShape(PhysicsObject physicsObject, ConvexHull shape)
	{ // OPTIMIZE: remove old shapes and don't always add the new ones if identical ones are already used (pass TypedIndex instead of TShape to the AddPhysicsObject method to accomodate more easily sharing the same shape between pipes of the same type)
		lock (SyncRoot)
		{
			PhysicsObjectNotDestroyedSafetyCheck(physicsObject);
			HandleExistsSafetyCheck(physicsObject.BodyHandle);

			// Something here causes bepu physics to get unstable.
			var oldShape = Simulation.Bodies[physicsObject.BodyHandle].Collidable.Shape;
			Simulation.Bodies[physicsObject.BodyHandle].SetShape(Simulation.Shapes.Add(shape));
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

			RigidPose pose = Simulation.Bodies[bodyHandle].Pose;

			return new(pose.Position.ToFixed<Vector3FixedDecimalInt4>(), pose.Orientation.ToFixed<QuaternionFixedDecimalInt4>());
		}
	}

	public void SetTransform(BodyHandle bodyHandle, Transform transform)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(bodyHandle);

			RigidPose pose = Simulation.Bodies[bodyHandle].Pose;

			pose.Position = transform.Position.ToVector3();

			pose.Orientation = transform.Rotation.ToQuaternion();

			Simulation.Bodies[bodyHandle].Pose = pose;
		}
	}

	private void HandleExistsSafetyCheck(BodyHandle bodyHandle)
	{
		if (!BodyHandleToPhysicsObject.ContainsKey(bodyHandle))
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