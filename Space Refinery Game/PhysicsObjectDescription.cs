using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;

namespace Space_Refinery_Game
{
	public struct PhysicsObjectDescription<TShape> where TShape : unmanaged, IShape
	{
		public IInformationProvider InformationProvider;

		public TShape Shape;

		public Transform InitialTransform;

		public FixedDecimalInt4 Mass;

		public bool Kinematic;

		public PhysicsObjectDescription(TShape shape, Transform initialTransform, FixedDecimalInt4 mass, bool kinematic, IInformationProvider informationProvider)
		{
			InformationProvider = informationProvider;
			Shape = shape;
			InitialTransform = initialTransform;
			Mass = mass;
			Kinematic = kinematic;
		}
	}
}