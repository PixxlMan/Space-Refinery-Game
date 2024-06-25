using Space_Refinery_Engine;
using System.Diagnostics;
using Veldrid;

namespace Space_Refinery_Engine.Renderer;

public sealed class ShaderLoader
{
	private Dictionary<string, Shader[]> shaderCache = new();

	private GraphicsWorld graphicsWorld;

	public ShaderLoader(GraphicsWorld graphicsWorld)
	{
		this.graphicsWorld = graphicsWorld;
	}

	public Shader[] LoadVertexFragmentCached(string shaderName)
	{
		if (!shaderCache.TryGetValue(shaderName, out Shader[]? shaders))
		{
			Logging.LogScopeStart($"Vertex-fragment shader loading '{shaderName}'");

			var path = Path.Combine(Environment.CurrentDirectory, "Graphics", "Shaders");
			shaders = Utils.LoadShaders(path, shaderName, graphicsWorld.Factory);

			shaderCache.Add(shaderName, shaders);

			Logging.Log($"Loaded and cached shader '{shaderName}' ('{shaders[0].Name}', '{shaders[1].Name}') with stages '{shaders[0].Stage}', '{shaders[1].Stage}' in {path}");
			Logging.LogScopeEnd();
		}

		Debug.Assert(shaders[0].Stage == ShaderStages.Vertex && shaders[1].Stage == ShaderStages.Fragment);
		return shaders;
	}

	public Shader LoadComputeCached(string shaderName)
	{
		Shader shader;
		if (shaderCache.TryGetValue(shaderName, out Shader[]? shaders))
		{
			shader = shaders[0];
		}
		else
		{
			Logging.LogScopeStart($"Compute shader loading '{shaderName}'");

			var path = Path.Combine(Environment.CurrentDirectory, "Graphics", "Shaders");
			shader = Utils.LoadShader(path, shaderName, graphicsWorld.Factory);

			shaderCache.Add(shaderName, [shader]);

			Logging.Log($"Loaded and cached shader '{shaderName}' ('{shader.Name}') with stage '{shader.Stage}' in {path}");
			Logging.LogScopeEnd();
		}

		Debug.Assert(shader.Stage == ShaderStages.Compute);
		return shader;
	}
}
