using BepuPhysics;
using Space_Refinery_Game_Renderer;

namespace Space_Refinery_Engine;

/// <remarks>
/// This class is thread safe.
/// </remarks>
public sealed class PhysicsObject
{
	public IInformationProvider InformationProvider => Entity.InformationProvider;

	public readonly Entity Entity;

	private bool enabled = true;
	public bool Enabled
	{
		get
		{
			lock (syncRoot)
			{
				return enabled;
			}
		}
		set
		{
			lock (syncRoot)
			{
				enabled = value;
			}
		}
	}

	public readonly PhysicsWorld World;

	public Transform Transform { get => World.GetTransform(BodyHandle); set { World.SetTransform(BodyHandle, value); } }

	public readonly BodyHandle BodyHandle;

	private readonly object syncRoot = new();

	/// <summary>
	/// Indicates whether this <see cref="PhysicsObject"/> is valid and can be used.
	/// </summary>
	public bool Valid => !Destroyed && World.HasHandle(BodyHandle);

	private bool destroyed = true;
	public bool Destroyed
	{
		get
		{
			lock (syncRoot)
			{
				return destroyed;
			}
		}
	}

	public PhysicsObject(PhysicsWorld world, BodyHandle bodyHandle, Entity entity)
	{
		World = world;
		BodyHandle = bodyHandle;
		Entity = entity;
	}

	public void Destroy()
	{
		lock (syncRoot)
		{
			if (!Valid)
			{
				return;
			}

			destroyed = true;

			World.DestroyPhysicsObject(this);
		}
	}
}