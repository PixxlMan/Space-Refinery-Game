﻿using BepuPhysics.Collidables;
using FixedPrecision;
using Space_Refinery_Engine.Renderer;
using System.Collections.Concurrent;
using System.Xml;
using Mesh = Space_Refinery_Engine.Renderer.Mesh;

namespace Space_Refinery_Engine;

public record struct Collider : IEntitySerializable
{
	public ColliderShapes Shape;
	public Vector3FixedDecimalInt4? Scale;
	public Transform Offset;
	public string? MeshPath;
	public Mesh? Mesh;

	private static ConcurrentDictionary<Mesh, ConvexHull> convexHulls = new();

	public Collider(ColliderShapes shape, Transform offset, Vector3FixedDecimalInt4? scale = null, Mesh? mesh = null) : this()
	{
		Shape = shape;
		Offset = offset;
		Scale = scale;
		Mesh = mesh;
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		Shape = reader.DeserializeEnum<ColliderShapes>(nameof(Shape));
		if (Shape is ColliderShapes.Box || Shape is ColliderShapes.Cylinder)
		{
			Scale = reader.DeserializeVector3FixedDecimalInt4(nameof(Scale));
		}
		else
		{
			MeshPath = reader.ReadString(nameof(MeshPath));
		}
		Offset = reader.DeserializeTransform(nameof(Offset));
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.Serialize(Shape, nameof(Shape));
		if (Shape is ColliderShapes.Box || Shape is ColliderShapes.Cylinder)
		{
			writer.Serialize(Scale!.Value, nameof(Scale));
		}
		else
		{
			writer.Serialize(MeshPath!, nameof(MeshPath));
		}
		writer.Serialize(Offset, nameof(Offset));
	}

	public readonly TypedIndex CreateShape(GameData gameData, out Transform offset)
	{
		offset = Offset;
		switch (Shape)
		{
			case ColliderShapes.Box:
				return gameData.PhysicsWorld.Simulation.Shapes.Add(new Box((float)Scale!.Value.X, (float)Scale.Value.Y, (float)Scale.Value.Z));
			case ColliderShapes.Cylinder:
				return gameData.PhysicsWorld.Simulation.Shapes.Add(new Cylinder((float)Scale!.Value.X, (float)Scale.Value.Y));
			case ColliderShapes.ConvexMesh:
				Mesh mesh;
				if (Mesh is null && MeshPath is null)
				{
					throw new Exception($"The ColliderShape for this collider requires a Mesh, but none was provided!");
				}
				else if (Mesh is null)
				{
					mesh = gameData.GraphicsWorld.MeshLoader.LoadCached(MeshPath!);
				}
				else
				{
					mesh = Mesh;
				}
				return gameData.PhysicsWorld.Simulation.Shapes.Add(GetConvexHullForMesh(mesh, gameData));
			default:
				throw new NotSupportedException();
		}
	}

	public static ConvexHull GetConvexHullForMesh(Mesh mesh, GameData gameData)
	{
		if (convexHulls.TryGetValue(mesh, out ConvexHull convexHull))
		{
			return convexHull;
		}
		else
		{
			ConvexHullHelper.ComputeHull(mesh.Points.AsSpan(), gameData.PhysicsWorld.BufferPool, out var hullData);

			convexHull = new(mesh.Points.AsSpan(), gameData.PhysicsWorld.BufferPool, out _);

			// It doesn't really matter if we can't add the hull to the cache, if it occurs it's likely that it was somehow added in a parallel process and it's not a problem.
			_ = convexHulls.TryAdd(mesh, convexHull);

			return convexHull;
		}
	}

	public void AddDebugObjects(Transform transform)
	{
		switch (Shape)
		{
			case ColliderShapes.Box:
				GameData.DebugRender.DrawCube(transform.PerformTransform(Offset), Veldrid.RgbaFloat.Orange, Scale!.Value);
				break;
			case ColliderShapes.Cylinder:
				throw new NotImplementedException();
			case ColliderShapes.ConvexMesh:
				GameData.DebugRender.DrawMesh(Mesh!, transform.PerformTransform(Offset), Veldrid.RgbaFloat.Orange);
				break;
			default:
				throw new GlitchInTheMatrixException();
		}
	}
}

[Serializable]
public enum ColliderShapes
{
	Box = 1,
	Cylinder = 2,
	ConvexMesh = 3,
}
