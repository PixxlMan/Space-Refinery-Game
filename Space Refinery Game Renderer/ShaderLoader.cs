using FXRenderer;
using Veldrid;

namespace Space_Refinery_Game_Renderer
{
	public class ShaderLoader
	{
		private Dictionary<string, Shader[]> shaderCache = new();

		private GraphicsWorld graphicsWorld;

		public ShaderLoader(GraphicsWorld graphicsWorld)
		{
			this.graphicsWorld = graphicsWorld;
		}

		public Shader[] LoadCached(string shaderName)
		{
			if (shaderCache.ContainsKey(shaderName))
			{
				return shaderCache[shaderName];
			}

			var shader = Utils.LoadShaders(Path.Combine(Environment.CurrentDirectory, "Shaders"), shaderName, graphicsWorld.Factory);

			shaderCache.Add(shaderName, shader);

			return shader;
		}
	}
}
