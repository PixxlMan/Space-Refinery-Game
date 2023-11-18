namespace Space_Refinery_Game_Renderer;

public sealed class MaterialLoader(GraphicsWorld graphicsWorld)
{
	private Dictionary<MaterialLoadingDescription, Material> materialCache = new();

	private GraphicsWorld graphicsWorld = graphicsWorld;

	public Material LoadCached(MaterialLoadingDescription materialTexturePaths)
	{
		if (materialCache.TryGetValue(materialTexturePaths, out Material? value))
		{
			return value;
		}

		var material = Material.LoadMaterial(
			graphicsWorld.GraphicsDevice,
			graphicsWorld.Factory,
			materialTexturePaths.Name,
			materialTexturePaths.DiffuseTexturePath,
			materialTexturePaths.MetallicTexturePath,
			materialTexturePaths.RoughnessTexturePath,
			materialTexturePaths.AmbientOcclusionTexturePath
			);

		materialCache.Add(materialTexturePaths, material);

		return material;
	}
}


public record struct MaterialLoadingDescription
	(
	string Name,
	string DiffuseTexturePath,
	string MetallicTexturePath,
	string RoughnessTexturePath,
	string AmbientOcclusionTexturePath
	)
{

}
