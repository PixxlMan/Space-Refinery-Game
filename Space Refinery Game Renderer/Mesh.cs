using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Utilities;

namespace Space_Refinery_Game_Renderer;

public sealed class Mesh
{
	private Mesh()
	{ }

	public Mesh(DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, IndexFormat indexFormat, uint indexCount)
	{
		VertexBuffer = vertexBuffer;
		IndexBuffer = indexBuffer;
		IndexFormat = indexFormat;
		IndexCount = indexCount;
	}

	public Vector3[] Points { get; private set; }

	public DeviceBuffer VertexBuffer { get; private set; }

	public DeviceBuffer IndexBuffer { get; private set; }

	public IndexFormat IndexFormat { get; private set; }

	public uint IndexCount { get; private set; }

	public static Mesh LoadMesh(GraphicsDevice gd, ResourceFactory factory, string path)
	{
		ObjParser objParser = new();

		ObjFile objFile = objParser.Parse(File.ReadAllLines(path));

		ConstructedMeshInfo meshInfo = objFile.GetFirstMesh();

		Mesh mesh = new();

		mesh.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)(meshInfo.Indices.Length * 4), BufferUsage.IndexBuffer));
		gd.UpdateBuffer(mesh.IndexBuffer, 0u, meshInfo.Indices);
		mesh.IndexCount = (uint)meshInfo.Indices.Length;

		mesh.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(meshInfo.Vertices.Length * 32), BufferUsage.VertexBuffer));
		gd.UpdateBuffer(mesh.VertexBuffer, 0u, meshInfo.Vertices);

		mesh.IndexFormat = IndexFormat.UInt16;

		mesh.Points = meshInfo.GetVertexPositions();

		return mesh;
	}

	public static Mesh CreateMesh(ushort[] indicies, VertexPositionNormalTexture[] verticies, GraphicsDevice gd, ResourceFactory factory)
	{
		Mesh mesh = new Mesh();

		mesh.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)(indicies.Length * 4), BufferUsage.IndexBuffer));
		gd.UpdateBuffer(mesh.IndexBuffer, 0u, indicies);
		mesh.IndexCount = (uint)indicies.Length;

		mesh.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(verticies.Length * 32), BufferUsage.VertexBuffer));
		gd.UpdateBuffer(mesh.VertexBuffer, 0u, verticies);

		mesh.IndexFormat = IndexFormat.UInt16;

		mesh.Points = GetVertexPositions(verticies);

		return mesh;
	}

	private static Vector3[] GetVertexPositions(VertexPositionNormalTexture[] verticies)
	{
		return verticies.Select(vpnt => vpnt.Position).ToArray();
	}
}
