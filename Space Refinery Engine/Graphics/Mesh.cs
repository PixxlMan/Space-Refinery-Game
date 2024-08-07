using System.Numerics;
using Veldrid;
using SharpGLTF.Scenes;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;

namespace Space_Refinery_Engine.Renderer;

public sealed class Mesh
{
	private Mesh()
	{ }

	public Mesh(string name, DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, IndexFormat indexFormat, uint indexCount)
	{
		Name = name;
		VertexBuffer = vertexBuffer;
		IndexBuffer = indexBuffer;
		IndexFormat = indexFormat;
		IndexCount = indexCount;
	}

	public string Name { get; private set; }

	public Vector3[] Points { get; private set; }

	public DeviceBuffer VertexBuffer { get; private set; }

	public DeviceBuffer IndexBuffer { get; private set; }

	public IndexFormat IndexFormat { get; private set; }

	public uint IndexCount { get; private set; }

	public FrontFace WindingOrder { get; private set; }

	public static Mesh LoadMesh(string path, GraphicsDevice gd, ResourceFactory factory)
	{
		SceneBuilder scene = SceneBuilder.LoadDefaultScene(path);

		var mesh = LoadMesh(scene.Instances[0], gd, factory);

		return mesh;
	}
	
	public static Mesh LoadMesh(InstanceBuilder instance, GraphicsDevice gd, ResourceFactory factory)
	{
		var meshInfo = instance.Content.GetGeometryAsset().Primitives.First();

		if (instance.Content.GetGeometryAsset().Primitives.Count > 1)
		{
			throw new NotSupportedException("Cannot load meshes containing several primitives");
		}

		var mesh = LoadMesh(instance.Name, meshInfo, gd, factory);

		return mesh;
	}

	public static Mesh LoadMesh(string name, IPrimitiveReader<MaterialBuilder> meshInfo, GraphicsDevice gd, ResourceFactory factory)
	{
		var verticies = new VertexData[meshInfo.Vertices.Count];

		for (int i = 0; i < meshInfo.Vertices.Count; i++)
		{
			IVertexBuilder? vertexBuilder = meshInfo.Vertices[i];
			var geometry = vertexBuilder.GetGeometry();

			var position = geometry.GetPosition();
			geometry.TryGetNormal(out var normal);
			var texCoords = vertexBuilder.GetMaterial().GetTexCoord(0);
			if (!geometry.TryGetTangent(out var tangent))
			{
				throw new NotSupportedException("Cannot load meshes that don't have exported tangents");
			}

			verticies[i] = new(position, normal, texCoords, new(tangent.X, tangent.Y, tangent.Z));
		}

		var mesh = CreateMesh(name, meshInfo.GetIndices().Select((i) => (ushort)i).ToArray(), verticies, FrontFace.CounterClockwise, gd, factory);

		return mesh;
	}

	public static Mesh CreateMesh(string name, ushort[] indicies, VertexData[] verticies, FrontFace windingOrder, GraphicsDevice gd, ResourceFactory factory)
	{
		Mesh mesh = new()
		{
			Name = name,
		};

		mesh.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)(indicies.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
		mesh.IndexBuffer.Name = $"{name} index buffer";
		gd.UpdateBuffer(mesh.IndexBuffer, 0u, indicies);
		mesh.IndexCount = (uint)indicies.Length;

		mesh.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(verticies.Length * VertexData.SizeInBytes), BufferUsage.VertexBuffer));
		mesh.VertexBuffer.Name = $"{name} vertex buffer";
		gd.UpdateBuffer(mesh.VertexBuffer, 0u, verticies);

		mesh.Points = GetVertexPositions(verticies);

		mesh.IndexFormat = IndexFormat.UInt16;
		mesh.WindingOrder = windingOrder;

		return mesh;
	}

	private static Vector3[] GetVertexPositions(VertexData[] verticies)
	{
		return verticies.Select(vpnt => vpnt.Position).ToArray();
	}
}
