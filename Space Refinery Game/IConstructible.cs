using FixedPrecision;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public interface IConstructible
	{
		public string TargetName { get; }

		public Func<Connector, IEntityType, int, FixedDecimalInt4, PhysicsWorld, GraphicsWorld, IConstruction> Build { get; }
	}
}