using BepuPhysics;
using BepuPhysics.Collidables;
using FXRenderer;

namespace Space_Refinery_Game
{
	public class PhysicsObject
	{
		public string Text = "lol";

		public readonly PhysicsWorld World;

		public Transform Transform => World.GetTransform(BodyHandle);

		public readonly BodyHandle BodyHandle;

		public PhysicsObject(PhysicsWorld world, BodyHandle bodyHandle)
		{
			World = world;
			BodyHandle = bodyHandle;
		}
	}
}