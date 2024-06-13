using BepuPhysics.Collidables;
using FixedPrecision;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Engine;

public struct PhysicsObjectDescription<TShape> where TShape : unmanaged, IShape
{
	public TShape Shape;

	public Transform InitialTransform;

	public FixedDecimalInt4 Mass;

	public bool Kinematic;

	public PhysicsObjectDescription(TShape shape, Transform initialTransform, FixedDecimalInt4 mass, bool kinematic)
	{
		Shape = shape;
		InitialTransform = initialTransform;
		Mass = mass;
		Kinematic = kinematic;
	}
}