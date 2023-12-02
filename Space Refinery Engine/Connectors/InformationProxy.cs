using BepuPhysics.Collidables;
using FXRenderer;

namespace Space_Refinery_Engine;

// todo: make thread safe

// TODO: Rename to physics proxy? that would more accurately describe the purpose and actual use case!
public sealed class InformationProxy
{
	public Entity ProxiedEntity { get; }

	private PhysicsObject? physicsObject;

	private bool enabled = false;

	public InformationProxy(Connector connector)
	{
		ProxiedEntity = connector;
	}

	public void Enable()
	{
		enabled = true;

		if (physicsObject is not null)
		{
			physicsObject.Enabled = true;
		}
	}

	public void Disable()
	{
		enabled = false;

		if (physicsObject is not null)
		{
			physicsObject.Enabled = false;
		}
	}

	public void SetPhysicsObjectState(Transform transform, ConvexHull shape, PhysicsWorld physicsWorld)
	{
		physicsObject?.Destroy();
		//if (physicsObject is null)
		//{
		var proxyPhysicsObject = new PhysicsObjectDescription<ConvexHull>(shape, transform, 0, true);

		physicsObject = physicsWorld.AddPhysicsObject(proxyPhysicsObject, ProxiedEntity);

		physicsObject.Enabled = enabled;
		//}
		//else
		//{
		// REMEMBER TO ALSO CHANGE THE TRANSFORM HERE!!
		//	physicsObject.World.ChangeShape(physicsObject, shape);
		//}
	}

	public void Destroy()
	{
		physicsObject?.Destroy();
	}

	public IInformationProvider InformationProvider => ProxiedEntity.InformationProvider;
}
