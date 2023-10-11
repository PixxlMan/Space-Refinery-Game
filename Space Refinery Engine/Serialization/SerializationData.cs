using Space_Refinery_Engine.Audio;
using Space_Refinery_Game_Renderer;
using System.Reflection;

namespace Space_Refinery_Engine;

public sealed record class SerializationData(GameData GameData, Assembly? AssemblyContext = null)
{
	public SerializationData(UI ui, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld, AudioWorld audioWorld, GameWorld gameWorld, MainGame mainGame, SerializationReferenceHandler referenceHandler, Settings settings, Assembly? assemblyContext = null)
		: this(new GameData(ui, physicsWorld, graphicsWorld, audioWorld, gameWorld, mainGame, referenceHandler, settings), assemblyContext)
	{ }

	public Action? DeserializationCompleteEvent;

	public void SerializationComplete()
	{
		DeserializationCompleteEvent?.Invoke();
	}
}
