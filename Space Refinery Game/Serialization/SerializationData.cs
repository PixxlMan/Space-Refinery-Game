using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class SerializationData // rename to deserializationdata?
	{
		public SerializationData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			GameData = new(ui, physicsWorld, graphicsWorld, gameWorld, mainGame, referenceHandler);
		}

		public SerializationData(GameData gameData)
		{
			GameData = gameData;
		}

		public GameData GameData;

		public event Action DeserializationCompleteEvent;

		public void SerializationComplete()
		{
			DeserializationCompleteEvent?.Invoke();
		}
	}
}
