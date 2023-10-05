using Space_Refinery_Engine.Audio;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Engine
{
	public sealed class SerializationData // rename to deserializationdata?
	{
		public SerializationData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, AudioWorld audioWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler, Settings settings)
		{
			GameData = new(ui, physicsWorld, graphicsWorld, audioWorld, gameWorld, mainGame, referenceHandler, settings);
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
