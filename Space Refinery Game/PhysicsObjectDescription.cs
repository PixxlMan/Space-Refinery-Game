using BepuPhysics.Collidables;
using FXRenderer;

namespace Space_Refinery_Game
{
	public struct PhysicsObjectDescription<TShape> where TShape : unmanaged, IShape
	{
		public TShape Shape;

		public Transform InitialTransform;

		public PhysicsObjectDescription(TShape shape, Transform initialTransform)
		{
			Shape = shape;
			InitialTransform = initialTransform;
		}
	}
}