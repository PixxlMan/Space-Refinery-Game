using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;

namespace Space_Refinery_Game
{
	public struct PhysicsObjectDescription<TShape> where TShape : unmanaged, IShape
	{
		public TShape Shape;

		public Transform InitialTransform;

		public FixedDecimalInt4 Mass;

		public PhysicsObjectDescription(TShape shape, Transform initialTransform, FixedDecimalInt4 mass)
		{
			Shape = shape;
			InitialTransform = initialTransform;
			Mass = mass;
		}
	}
}