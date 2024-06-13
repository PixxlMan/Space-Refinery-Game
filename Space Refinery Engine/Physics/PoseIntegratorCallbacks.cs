﻿using BepuPhysics;
using BepuUtilities;
using System.Numerics;

namespace Space_Refinery_Engine;

public partial class PhysicsWorld
{
	//Note that the engine does not require any particular form of gravity- it, like all the contact callbacks, is managed by a callback.
	public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		Vector<float> linearDampingDt;
		Vector<float> angularDampingDt;
		Vector3Wide gravityWideDt;

		Vector3 gravity = new(0, -9.82f, 0);
		float linearDamping = 0.1f;
		float angularDamping = 0.01f;

		public PoseIntegratorCallbacks(Vector3 gravity, float linearDamping, float angularDamping)
		{
			this.gravity = gravity;
			this.linearDamping = linearDamping;
			angularDamping = angularDamping;
		}

		/// <summary>
		/// Performs any required initialization logic after the Simulation instance has been constructed.
		/// </summary>
		/// <param name="simulation">Simulation that owns these callbacks.</param>
		public void Initialize(Simulation simulation)
		{
			//In this demo, we don't need to initialize anything.
			//If you had a simulation with per body gravity stored in a CollidableProperty<T> or something similar, having the simulation provided in a callback can be helpful.
		}

		/// <summary>
		/// Gets how the pose integrator should handle angular velocity integration.
		/// </summary>
		public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

		/// <summary>
		/// Gets whether the integrator should use substepping for unconstrained bodies when using a substepping solver.
		/// If true, unconstrained bodies will be integrated with the same number of substeps as the constrained bodies in the solver.
		/// If false, unconstrained bodies use a single step of length equal to the dt provided to Simulation.Timestep. 
		/// </summary>
		public readonly bool AllowSubstepsForUnconstrainedBodies => false;

		/// <summary>
		/// Gets whether the velocity integration callback should be called for kinematic bodies.
		/// If true, IntegrateVelocity will be called for bundles including kinematic bodies.
		/// If false, kinematic bodies will just continue using whatever velocity they have set.
		/// Most use cases should set this to false.
		/// </summary>
		public readonly bool IntegrateVelocityForKinematics => false;

		/// <summary>
		/// Callback invoked ahead of dispatches that may call into <see cref="IntegrateVelocity"/>.
		/// It may be called more than once with different values over a frame. For example, when performing bounding box prediction, velocity is integrated with a full frame time step duration.
		/// During substepped solves, integration is split into substepCount steps, each with fullFrameDuration / substepCount duration.
		/// The final integration pass for unconstrained bodies may be either fullFrameDuration or fullFrameDuration / substepCount, depending on the value of AllowSubstepsForUnconstrainedBodies. 
		/// </summary>
		/// <param name="dt">Current integration time step duration.</param>
		/// <remarks>This is typically used for precomputing anything expensive that will be used across velocity integration.</remarks>
		public void PrepareForIntegration(float dt)
		{
			linearDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - linearDamping, 0, 1), dt));
			angularDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - angularDamping, 0, 1), dt));
			gravityWideDt = Vector3Wide.Broadcast(gravity * dt);
		}

		/// <summary>
		/// Callback for a bundle of bodies being integrated.
		/// </summary>
		/// <param name="bodyIndices">Indices of the bodies being integrated in this bundle.</param>
		/// <param name="position">Current body positions.</param>
		/// <param name="orientation">Current body orientations.</param>
		/// <param name="localInertia">Body's current local inertia.</param>
		/// <param name="integrationMask">Mask indicating which lanes are active in the bundle. Active lanes will contain 0xFFFFFFFF, inactive lanes will contain 0.</param>
		/// <param name="workerIndex">Index of the worker thread processing this bundle.</param>
		/// <param name="dt">Durations to integrate the velocity over. Can vary over lanes.</param>
		/// <param name="velocity">Velocity of bodies in the bundle. Any changes to lanes which are not active by the integrationMask will be discarded.</param>
		public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
		{
			velocity.Linear = (velocity.Linear + gravityWideDt) * linearDampingDt;
			velocity.Angular = velocity.Angular * angularDampingDt;
		}
	}
}