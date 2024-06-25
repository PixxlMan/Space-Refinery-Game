using BepuPhysics;
using Space_Refinery_Engine.Renderer;
using System.Diagnostics.CodeAnalysis;

namespace Space_Refinery_Engine;

/// <remarks>
/// This class is thread safe.
/// </remarks>
public sealed class PhysicsObject
{
	public IInformationProvider InformationProvider => Entity.InformationProvider;

	public Entity Entity { get; private set; }

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

	private bool recievesRaycasts = true;
	public bool RecievesRaycasts
	{
		get
		{
			lock (syncRoot)
			{
				return recievesRaycasts;
			}
		}
		set
		{
			lock (syncRoot)
			{
				recievesRaycasts = value;
			}
		}
	}

	public PhysicsWorld World { get; private set; }

	public Transform Transform { get => World.GetTransform(this); set { World.SetTransform(this, value); } }

	public bool IsDynamic => BodyHandle.HasValue;
	public bool IsStatic => StaticHandle.HasValue;

	[MemberNotNull(nameof(IsDynamic))]
	public BodyHandle? BodyHandle { get; private set; }
	[MemberNotNull(nameof(IsStatic))]
	public StaticHandle? StaticHandle { get; private set; }

	private readonly object syncRoot = new();

	/// <summary>
	/// Indicates whether this <see cref="PhysicsObject"/> is valid and can be used.
	/// </summary>
	public bool Valid => !Destroyed &&
		(World.HasHandle(BodyHandle) || World.HasHandle(StaticHandle)) &&
		((BodyHandle.HasValue && !StaticHandle.HasValue) || (StaticHandle.HasValue && !BodyHandle.HasValue));

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

	internal PhysicsObject(PhysicsWorld world, BodyHandle bodyHandle, Entity entity)
	{
		World = world;
		BodyHandle = bodyHandle;
		Entity = entity;
	}

	internal PhysicsObject(PhysicsWorld world, StaticHandle staticHandle, Entity entity)
	{
		World = world;
		StaticHandle = staticHandle;
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