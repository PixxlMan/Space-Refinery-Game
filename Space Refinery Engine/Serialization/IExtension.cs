using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Space_Refinery_Engine;

/// <summary>
/// Provides access to several callbacks useful for controlling initalization of the engine. Can be used as a centralized location for extension-spanning state.
/// </summary>
public interface IExtension : ISerializableReference
{
	/// <summary>
	/// Called after all initialization, right before starting the main update and tick loops.
	/// </summary>
	public abstract void Start(GameData gameData);

	/// <summary>
	/// Allows controlling the set up of the physics system.
	/// Returning <see cref="true"/> indicates that the <see cref="out"/> parameters should be used in the physics system; returning <see cref="false"/> indicates that the <see cref="out"/> parameters should be ignored.
	/// Only one extension may set up the physics system using this method, all other extensions must return <see cref="false"/> in this method.
	/// </summary>
	/// <param name="simulation">The <see cref="Simulation"/> to be used in the physics system, if the method returns <see cref="true"/>.</param>
	/// <param name="bufferPool">The <see cref="BufferPool"/> to be used in the physics system, if the method returns <see cref="true"/>.</param>
	/// <param name="threadDispatcher">The <see cref="IThreadDispatcher"/> to be used in the physics system, if the method returns <see cref="true"/>.</param>
	/// <returns><see cref="true"/> or <see cref="false"/> to indicate whether to use out parameters or not.</returns>
	public virtual bool SetUpPhysics(out Simulation? simulation, out BufferPool? bufferPool, out IThreadDispatcher? threadDispatcher)
	{
		simulation = null;
		bufferPool = null;
		threadDispatcher = null;

		return false;
	}

	/// <summary>
	/// Called during initialization, while the GlobalReferenceHandler is still open for eventual references.
	/// </summary>
	/// <param name="globalReferenceHandler">The GlobalReferenceHandler.</param>
	/// <param name="gameData">GameData with yet incomplete state.</param>
	public virtual void OnGlobalReferenceHandlerDeserialization(SerializationReferenceHandler globalReferenceHandler, GameData gameData)
	{

	}
}