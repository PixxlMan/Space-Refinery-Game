using Space_Refinery_Game.Audio;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Game
{
	public sealed class SerializationData // rename to deserializationdata?
	{
		public SerializationData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, AudioWorld audioWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler)
		{
			GameData = new(ui, physicsWorld, graphicsWorld, audioWorld, gameWorld, mainGame, referenceHandler);
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
