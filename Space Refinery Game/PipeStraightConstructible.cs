using FixedPrecision;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public class PipeStraightConstructible : IConstructible
	{
		public string TargetName => "Pipe Straight";

		public Func<Connector, IEntityType, int, FixedDecimalInt4, PhysicsWorld, GraphicsWorld, IConstruction> Build => Pipe.Build;
	}
}