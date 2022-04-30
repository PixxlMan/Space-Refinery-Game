﻿using BepuPhysics;
using BepuPhysics.Collidables;
using FXRenderer;

namespace Space_Refinery_Game
{
	public class PhysicsObject
	{
		public IInformationProvider InformationProvider => Entity.InformationProvider;

		public readonly Entity Entity;

		public bool Enabled = true;

		public readonly PhysicsWorld World;

		public Transform Transform => World.GetTransform(BodyHandle);

		public readonly BodyHandle BodyHandle;

		public PhysicsObject(PhysicsWorld world, BodyHandle bodyHandle, Entity entity)
		{
			World = world;
			BodyHandle = bodyHandle;
			Entity = entity;
		}

		public void Destroy()
		{
			World.DestroyPhysicsObject(this);
		}
	}
}