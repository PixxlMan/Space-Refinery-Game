using Space_Refinery_Engine.Audio;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Engine;

public sealed record class SerializationData(GameData GameData, string? BasePathForAssetDeserialization = null)
{
	public SerializationData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, AudioWorld audioWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler, Settings settings, string? basePathForAssetDeserialization)
		: this(new GameData(ui, physicsWorld, graphicsWorld, audioWorld, gameWorld, mainGame, referenceHandler, settings), basePathForAssetDeserialization)
	{ }

	public event Action? DeserializationCompleteEvent;

	public void SerializationComplete()
	{
		DeserializationCompleteEvent?.Invoke();
	}
}
