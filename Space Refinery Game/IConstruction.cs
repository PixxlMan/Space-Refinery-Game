using FixedPrecision;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public interface IConstruction//<TConnector> where TConnector : Connector
	{
		public static abstract IConstruction/*<TConnector>*/ Build(/*TConnector*/ Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalInt4 rotation, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld);

		public void Deconstruct();

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}