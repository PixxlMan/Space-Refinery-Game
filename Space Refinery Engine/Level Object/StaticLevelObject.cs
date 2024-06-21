using BepuPhysics;
using BepuPhysics.Collidables;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Engine;

public abstract class StaticLevelObject : LevelObject
{
	protected override PhysicsObject CreatePhysicsObject()
	{
		var shape = LevelObjectType.Collider.CreateShape(gameData, out var offset);
		Transform physicsTransform = Transform.PerformTransform(offset);
		StaticDescription staticDescription = new(physicsTransform.Position.ToVector3(), physicsTransform.Rotation.ToQuaternion(), shape, ContinuousDetection.Discrete);

		return gameData.PhysicsWorld.AddPhysicsObject(staticDescription, this);
	}
}
