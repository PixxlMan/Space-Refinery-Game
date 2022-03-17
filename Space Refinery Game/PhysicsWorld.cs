using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Space_Refinery_Game
{
	public partial class PhysicsWorld
	{
		private BufferPool bufferPool;

		private Simulation simulation;

		private IThreadDispatcher threadDispatcher;

		public void SetUp()
		{
			//The buffer pool is a source of raw memory blobs for the engine to use.
			bufferPool = new BufferPool();

			//The following sets up a simulation with the callbacks defined above, and tells it to use 8 velocity iterations per substep and only one substep per solve.
			//It uses the default SubsteppingTimestepper. You could use a custom ITimestepper implementation to customize when stages run relative to each other, or to insert more callbacks.         
			simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));

			//Any IThreadDispatcher implementation can be used for multithreading. Here, we use the BepuUtilities.ThreadDispatcher implementation.
			threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
		}

		public void Run()
		{
			while (true)
			{
				Thread.Sleep(16);

				simulation.Timestep(0.016f, threadDispatcher);
			}
		}

		public PhysicsObject AddPhysicsObject<TShape>(PhysicsObjectDescription<TShape> physicsObjectDescription) where TShape : unmanaged, IConvexShape
		{
			var inertia = physicsObjectDescription.Shape.ComputeInertia(100);
			BodyHandle bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(new Vector3(0, 20, 0), inertia, simulation.Shapes.Add(physicsObjectDescription.Shape), 0.01f));

			return new PhysicsObject(this, bodyHandle);
		}

		public Transform GetTransform(BodyHandle bodyHandle)
		{
			RigidPose pose = simulation.Bodies[bodyHandle].Pose;

			return new(pose.Position.ToFixed<Vector3FixedDecimalInt4>(), pose.Orientation.ToFixed<QuaternionFixedDecimalInt4>());
		}
	}
}