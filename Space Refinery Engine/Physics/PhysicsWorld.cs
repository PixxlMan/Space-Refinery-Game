using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using FixedPrecision;
using Space_Refinery_Game.Renderer;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;

namespace Space_Refinery_Engine;

public sealed partial class PhysicsWorld
{
	private Dictionary<BodyHandle, PhysicsObject> bodyHandleToPhysicsObject = [];
	public ReadOnlyDictionary<BodyHandle, PhysicsObject> BodyHandleToPhysicsObject { get; private set; }
	
	private Dictionary<StaticHandle, PhysicsObject> staticHandleToPhysicsObject = [];
	public ReadOnlyDictionary<StaticHandle, PhysicsObject> StaticHandleToPhysicsObject { get; private set; }

	public object SyncRoot = new();

	public BufferPool BufferPool { get; private set; }

	public Simulation Simulation { get; private set; }

	public IThreadDispatcher ThreadDispatcher { get; private set; }

	public event Action<IntervalUnit>? CollectPhysicsPerformanceData;

	private string responseSpinner = "_";
	public string ResponseSpinner { get { lock (responseSpinner) return responseSpinner; } } // The response spinner can be used to visually show that the thread is running correctly and is not stopped or deadlocked.

	private GameData gameData;

	public PhysicsWorld()
	{
		BodyHandleToPhysicsObject = bodyHandleToPhysicsObject.AsReadOnly();
		StaticHandleToPhysicsObject = staticHandleToPhysicsObject.AsReadOnly();
	}

	public void SetUp(Simulation simulation, BufferPool bufferPool, IThreadDispatcher threadDispatcher, GameData gameData)
	{
		lock (SyncRoot)
		{
			Simulation = simulation;
			BufferPool = bufferPool;
			ThreadDispatcher = threadDispatcher;
			this.gameData = gameData;
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
				if (!gameData.Paused)
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
			}
		}))
		{ Name = "Physics Update Thread" };

		thread.Start();
	}

	public Action<IntervalUnit>? OnPhysicsUpdate;

	public bool HasHandle(BodyHandle? bodyHandle)
	{
		return bodyHandle.HasValue && BodyHandleToPhysicsObject.ContainsKey(bodyHandle.Value);
	}

	public bool HasHandle(StaticHandle? staticHandle)
	{
		return staticHandle.HasValue && StaticHandleToPhysicsObject.ContainsKey(staticHandle.Value);
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

			PhysicsObject physicsObject = new(this, bodyHandle, entity);

			bodyHandleToPhysicsObject.Add(bodyHandle, physicsObject);

			return physicsObject;
		}
	}

	public PhysicsObject AddPhysicsObject(BodyDescription bodyDescription, Entity entity)
	{
		lock (SyncRoot)
		{
			var bodyHandle = Simulation.Bodies.Add(bodyDescription);

			PhysicsObject physicsObject = new(this, bodyHandle, entity);

			bodyHandleToPhysicsObject.Add(bodyHandle, physicsObject);

			return physicsObject;
		}
	}

	public PhysicsObject AddPhysicsObject(StaticDescription staticDescription, Entity entity)
	{
		lock (SyncRoot)
		{
			var staticHandle = Simulation.Statics.Add(staticDescription);

			PhysicsObject physicsObject = new(this, staticHandle, entity);

			staticHandleToPhysicsObject.Add(staticHandle, physicsObject);

			return physicsObject;
		}
	}

	public void DestroyPhysicsObject(PhysicsObject physicsObject)
	{
		if (physicsObject.Destroyed)
		{
			return;
		}

		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(physicsObject);

			if (physicsObject.IsDynamic)
			{
				bodyHandleToPhysicsObject.Remove(physicsObject.BodyHandle!.Value);

				Simulation.Bodies.Remove(physicsObject.BodyHandle!.Value);
			}
			else if (physicsObject.IsStatic)
			{
				staticHandleToPhysicsObject.Remove(physicsObject.StaticHandle!.Value);

				Simulation.Statics.Remove(physicsObject.StaticHandle!.Value);
			}
			else
			{
				throw new GlitchInTheMatrixException();
			}

			physicsObject.Destroy();
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
			Simulation.Clear();
			Simulation.Bodies.Clear();
		}
	}

	public void AddImpulse(PhysicsObject physicsObject, Vector3FixedDecimalInt4 impulse)
	{
		lock (SyncRoot)
		{
			if (!physicsObject.IsDynamic)
			{
				throw new Exception("Cannot add impulse to a physics object which is not dynamic!");
			}

			Vector3 floatImpulse = impulse.ToVector3();
			Vector3 offset = physicsObject.Transform.Position.ToVector3();
			new BodyReference(physicsObject.BodyHandle!.Value, Simulation.Bodies).ApplyImpulse(in floatImpulse, in offset);
		}
	}

	public void SetShape(PhysicsObject physicsObject, ConvexHull shape)
	{ // OPTIMIZE: remove old shapes and don't always add the new ones if identical ones are already used (pass TypedIndex instead of TShape to the AddPhysicsObject method to accomodate more easily sharing the same shape between pipes of the same type)
		lock (SyncRoot)
		{
			PhysicsObjectNotDestroyedSafetyCheck(physicsObject);
			HandleExistsSafetyCheck(physicsObject);

			if (physicsObject.IsDynamic)
			{
				// Something here causes bepu physics to get unstable.
				var oldShape = Simulation.Bodies[physicsObject.BodyHandle!.Value].Collidable.Shape;
				Simulation.Bodies[physicsObject.BodyHandle!.Value].SetShape(Simulation.Shapes.Add(shape));
				//simulation.Shapes.RecursivelyRemoveAndDispose(oldShape, bufferPool);
				//simulation.Shapes.RemoveAndDispose(oldShape, bufferPool);
				//simulation.Shapes.Remove(oldShape); // Least broken option
			}
			else if (physicsObject.IsStatic)
			{

			}
			else
			{
				throw new GlitchInTheMatrixException();
			}
		}
	}

	public Transform GetTransform(PhysicsObject physicsObject)
	{
		if (physicsObject.IsDynamic)
		{
			return GetTransform(physicsObject.BodyHandle!.Value);
		}
		else if (physicsObject.IsStatic)
		{
			return GetTransform(physicsObject.StaticHandle!.Value);
		}
		else
		{
			throw new GlitchInTheMatrixException();
		}
	}

	public void SetTransform(PhysicsObject physicsObject, Transform transform)
	{
		if (physicsObject.IsDynamic)
		{
			SetTransform(physicsObject.BodyHandle!.Value, transform);
		}
		else if (physicsObject.IsStatic)
		{
			SetTransform(physicsObject.StaticHandle!.Value, transform);
		}
		else
		{
			throw new GlitchInTheMatrixException();
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

	public Transform GetTransform(StaticHandle staticHandle)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(staticHandle);

			RigidPose pose = Simulation.Statics[staticHandle].Pose;

			return new(pose.Position.ToFixed<Vector3FixedDecimalInt4>(), pose.Orientation.ToFixed<QuaternionFixedDecimalInt4>());
		}
	}

	public void SetTransform(StaticHandle staticHandle, Transform transform)
	{
		lock (SyncRoot)
		{
			HandleExistsSafetyCheck(staticHandle);

			RigidPose pose = Simulation.Statics[staticHandle].Pose;

			pose.Position = transform.Position.ToVector3();

			pose.Orientation = transform.Rotation.ToQuaternion();

			Simulation.Statics[staticHandle].Pose = pose;
		}
	}

	private void HandleExistsSafetyCheck(PhysicsObject physicsObject)
	{
		if (physicsObject.IsDynamic)
		{
			HandleExistsSafetyCheck(physicsObject.BodyHandle!.Value);
		}
		else if (physicsObject.IsStatic)
		{
			HandleExistsSafetyCheck(physicsObject.StaticHandle!.Value);
		}
		else
		{
			throw new GlitchInTheMatrixException();
		}
	}

	private void HandleExistsSafetyCheck(BodyHandle bodyHandle)
	{
		if (!BodyHandleToPhysicsObject.ContainsKey(bodyHandle))
		{
			throw new ArgumentException($"{nameof(BodyHandle)} is either invalid or the corresponding object is destroyed!", nameof(bodyHandle));
		}
	}
	
	private void HandleExistsSafetyCheck(StaticHandle staticHandle)
	{
		if (!StaticHandleToPhysicsObject.ContainsKey(staticHandle))
		{
			throw new ArgumentException($"{nameof(StaticHandle)} is either invalid or the corresponding object is destroyed!", nameof(staticHandle));
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