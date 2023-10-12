using Space_Refinery_Engine.Audio;
using Space_Refinery_Game_Renderer;
using System.Reflection;

namespace Space_Refinery_Engine;

public sealed record class SerializationData(GameData GameData, Extension ExtensionContext)
{
	public SerializationData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, AudioWorld audioWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler, Settings settings, Extension extensionContext = null)
		: this(new GameData(ui, physicsWorld, graphicsWorld, audioWorld, gameWorld, mainGame, referenceHandler, settings), extensionContext)
	{ }

	public event Action? DeserializationCompleteEvent;

	public void SerializationComplete()
	{
		DeserializationCompleteEvent?.Invoke();
	}
}
