namespace Space_Refinery_Game_Renderer;

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

		var mesh = Mesh.LoadMesh(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, path);

		AddCache(path, mesh);

		return mesh;
	}

	public void AddCache(string name, Mesh mesh)
	{
		meshCache.Add(name, mesh);
	}

	public bool TryGetCached(string name, out Mesh? mesh)
	{
		if (meshCache.ContainsKey(name))
		{
			mesh = meshCache[name];

			return true;
		}

		mesh = null;

		return false;
	}
}
