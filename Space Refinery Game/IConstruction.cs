using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public interface IConstruction//<TConnector> where TConnector : Connector
	{
		public static abstract IConstruction/*<TConnector>*/ Build(/*TConnector*/ Connector connector, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld);

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}