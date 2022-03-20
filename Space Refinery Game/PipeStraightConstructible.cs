using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public class PipeStraightConstructible : IConstructible
	{
		public string TargetName => "Pipe Straight";

		public Func<Connector, PhysicsWorld, GraphicsWorld, IConstruction> Build => PipeStraight.Build;
	}
}