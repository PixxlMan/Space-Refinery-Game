using Space_Refinery_Engine.Audio;
using Space_Refinery_Game.Renderer;

namespace Space_Refinery_Engine;

public sealed record class SerializationData(GameData GameData, string? BasePathForAssetDeserialization = null)
{
	public SerializationData(GraphicsWorld graphicsWorld, PhysicsWorld physicsWorld, InputUpdate inputUpdate, AudioWorld audioWorld, Settings settings, Game game, UI ui, string? basePathForAssetDeserialization)
		: this(new GameData(graphicsWorld, physicsWorld, inputUpdate, audioWorld, settings, game, ui), basePathForAssetDeserialization)
	{ }

	public event Action? DeserializationCompleteEvent;

	public void SerializationComplete()
	{
		DeserializationCompleteEvent?.Invoke();
	}
}
