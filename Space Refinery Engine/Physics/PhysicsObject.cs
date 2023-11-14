using BepuPhysics;
using FXRenderer;

namespace Space_Refinery_Engine
{
	public sealed class PhysicsObject
	{
		public IInformationProvider InformationProvider => Entity.InformationProvider;

		public readonly Entity Entity;

		public bool Enabled = true;

		public readonly PhysicsWorld World;

		public Transform Transform { get => World.GetTransform(BodyHandle); set { World.SetTransform(BodyHandle, value); } }

		public readonly BodyHandle BodyHandle;

		/// <summary>
		/// Indicates whether this <see cref="PhysicsObject"/> is valid and can be used.
		/// </summary>
		public bool Valid => !Destroyed && World.HasHandle(BodyHandle);

		public bool Destroyed;

		public PhysicsObject(PhysicsWorld world, BodyHandle bodyHandle, Entity entity)
		{
			World = world;
			BodyHandle = bodyHandle;
			Entity = entity;
		}

		public void Destroy()
		{
			if (Destroyed)
			{
				return;
			}

			Destroyed = true;

			World.DestroyPhysicsObject(this);
		}
	}
}