using Space_Refinery_Game_Renderer;
using Space_Refinery_Engine;
using FixedPrecision;
using BepuPhysics;
using BepuUtilities.Memory;
using BepuUtilities;
using static Space_Refinery_Engine.PhysicsWorld;
using System.Numerics;
using System.Xml;

namespace Space_Refinery_Game;

public class SpaceRefineryGameExtension : IExtension
{
	public SerializableReference SerializableReference => "SpaceRefineryGameExtension";

	public void Start(GameData gameData)
	{
		gameData.GraphicsWorld.Window.SdlWindow.Title = "Space Refinery Game";

		Starfield.CreateAndAdd(gameData.GraphicsWorld);

		Pipe.Create(PipeType.PipeTypes["Straight Pipe"], new Transform(new(0, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), gameData, gameData.Game.GameReferenceHandler);

		gameData.Settings.RegisterToSettingValue<SwitchSettingValue>("Use Celcius", (v) => FormatUnit.UseCelcius = v);
		gameData.Settings.RegisterToSettingValue<SwitchSettingValue>("Use Pascal", (v) => FormatUnit.UsePascal = v);

		Player.Create(gameData);
	}

	public bool SetUpPhysics(out Simulation? simulation, out BufferPool? bufferPool, out IThreadDispatcher? threadDispatcher)
	{
		//The buffer pool is a source of raw memory blobs for the engine to use.
		bufferPool = new BufferPool();

		//The following sets up a simulation with the callbacks defined above, and tells it to use 8 velocity iterations per substep and only one substep per solve.
		//It uses the default SubsteppingTimestepper. You could use a custom ITimestepper implementation to customize when stages run relative to each other, or to insert more callbacks.         
		simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(Vector3.Zero, 0, 0), new SolveDescription(8, 1));

		//Any IThreadDispatcher implementation can be used for multithreading. Here, we use the BepuUtilities.ThreadDispatcher implementation.
		threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);

		return true;
	}

	public void SerializeState(XmlWriter writer)
	{
		throw new NotSupportedException();
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		var reference = reader.ReadReference("Reference");
		if (reference != SerializableReference)
		{
			throw new Exception($"Incorrect reference assigned to extension. Reference should be {SerializableReference}, was {reference}.");
		}
	}
}
