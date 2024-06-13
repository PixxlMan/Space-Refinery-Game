using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Utilities;

namespace Space_Refinery_Game_Renderer;

public class Mesh
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

	public Vector3[] Points;

	public DeviceBuffer VertexBuffer;

	public DeviceBuffer IndexBuffer;

	public IndexFormat IndexFormat;

	public uint IndexCount;

	public static Mesh LoadMesh(GraphicsDevice gd, ResourceFactory factory, string path)
	{
		ObjParser objParser = new ObjParser();

		ObjFile objFile = objParser.Parse(File.ReadAllLines(path));

		ConstructedMeshInfo meshInfo = objFile.GetFirstMesh();

		Mesh mesh = new Mesh();

		mesh.IndexBuffer = factory.CreateBuffer(new BufferDescription((uint)(meshInfo.Indices.Length * 4), BufferUsage.IndexBuffer));
		gd.UpdateBuffer(mesh.IndexBuffer, 0u, meshInfo.Indices);
		mesh.IndexCount = (uint)meshInfo.Indices.Length;

		mesh.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(meshInfo.Vertices.Length * 32), BufferUsage.VertexBuffer));
		gd.UpdateBuffer(mesh.VertexBuffer, 0u, meshInfo.Vertices);

		mesh.IndexFormat = IndexFormat.UInt16;

		mesh.Points = meshInfo.GetVertexPositions();

		return mesh;
	}
}
