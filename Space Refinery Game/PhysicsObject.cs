using BepuPhysics;
using BepuPhysics.Collidables;
using FXRenderer;

namespace Space_Refinery_Game
{
	public class PhysicsObject
	{
		public IInformationProvider InformationProvider;

		public readonly PhysicsWorld World;

		public Transform Transform => World.GetTransform(BodyHandle);

		public readonly BodyHandle BodyHandle;

		public PhysicsObject(PhysicsWorld world, BodyHandle bodyHandle, IInformationProvider informationProvider)
		{
			World = world;
			BodyHandle = bodyHandle;
			InformationProvider = informationProvider;
		}
	}
}