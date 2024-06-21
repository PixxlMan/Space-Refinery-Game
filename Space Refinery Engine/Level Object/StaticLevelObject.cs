using BepuPhysics;
using BepuPhysics.Collidables;

namespace Space_Refinery_Engine;

public abstract class StaticLevelObject : LevelObject
{
	protected override PhysicsObject CreatePhysicsObject()
	{
		var shape = LevelObjectType.Collider.CreateShape(gameData);
		StaticDescription staticDescription = new(Transform.Position.ToVector3(), Transform.Rotation.ToQuaternion(), shape, ContinuousDetection.Discrete);

		return gameData.PhysicsWorld.AddPhysicsObject(staticDescription, this);
	}
}
