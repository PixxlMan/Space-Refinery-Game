using SharpGLTF.Scenes;
using System.Diagnostics.CodeAnalysis;

namespace Space_Refinery_Engine.Renderer;

public sealed class MeshLoader
{
	private Dictionary<string, Mesh> meshCache = new();

	private GraphicsWorld graphicsWorld;

	public MeshLoader(GraphicsWorld graphicsWorld)
	{
		this.graphicsWorld = graphicsWorld;
	}

	public Mesh LoadCached(string path)
	{
		path = Path.GetFullPath(path);

		if (meshCache.TryGetValue(path, out Mesh? value))
		{
			return value;
		}

		var mesh = Mesh.LoadMesh(path, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);

		AddCache(path, mesh);

		return mesh;
	}

	/// <summary>
	/// Load and caches all meshes from a given scene.
	/// </summary>
	public void LoadAndCacheAll(SceneBuilder sceneBuilder)
	{
		foreach (InstanceBuilder instance in sceneBuilder.Instances)
		{
			string name = instance.Name.Split('.')[0];
			var mesh = Mesh.LoadMesh(instance, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
			AddCache(name, mesh);
		}
	}

	public void AddCache(string name, Mesh mesh)
	{
		meshCache.Add(name, mesh);
	}

	public bool TryGetCached(string name, [NotNullWhen(false)] out Mesh? mesh)
	{
		return meshCache.TryGetValue(name, out mesh!);
	}
}
