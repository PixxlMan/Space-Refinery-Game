using SharpGLTF.Materials;
using Space_Refinery_Utilities;

namespace Space_Refinery_Game_Renderer;

public sealed class MaterialLoader(GraphicsWorld graphicsWorld)
{
	private Dictionary<string, Material> materialCache = new();

	private GraphicsWorld graphicsWorld = graphicsWorld;

	public Material LoadCached(MaterialLoadingDescription materialTexturePaths)
	{
		if (materialCache.TryGetValue(materialTexturePaths.Name, out Material? material))
		{
			return material;
		}

		material = Material.LoadMaterial(
			graphicsWorld.GraphicsDevice,
			graphicsWorld.Factory,
			materialTexturePaths.Name,
			materialTexturePaths.DiffuseTexturePath,
			materialTexturePaths.MetallicTexturePath,
			materialTexturePaths.RoughnessTexturePath,
			materialTexturePaths.AmbientOcclusionTexturePath
			);

		materialCache.Add(materialTexturePaths.Name, material);

		return material;
	}

	public Material LoadGLTFMaterial(MaterialBuilder gltfMaterial)
	{
		if (materialCache.TryGetValue(gltfMaterial.Name, out Material? material))
		{
			return material;
		}

		Logging.Log($"Loading resources for material {gltfMaterial.Name} from GLTF material");

		var diffuseImage = gltfMaterial.GetChannel(KnownChannel.BaseColor).GetValidTexture().PrimaryImage.Content;
		var metallicImage = gltfMaterial.GetChannel(KnownChannel.Normal).GetValidTexture().PrimaryImage.Content;
		var roughnessImage = gltfMaterial.GetChannel(KnownChannel.MetallicRoughness).GetValidTexture().PrimaryImage.Content;
		//var ambientOcclusionImage = gltfMaterial.GetChannel(KnownChannel.Normal).GetValidTexture().PrimaryImage.Content;

		material = Material.FromTextures(
			graphicsWorld.GraphicsDevice,
			graphicsWorld.Factory,
			gltfMaterial.Name,
			Material.CreateTextureFromBytes(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, diffuseImage.Content.Span),
			Material.CreateTextureFromBytes(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, metallicImage.Content.Span),
			Material.CreateTextureFromBytes(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, roughnessImage.Content.Span),
			RenderingResources.DefaultTexture//Material.CreateTextureFromBytes(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, ambientOcclusionImage.Content.Span)
			);

		materialCache.Add(gltfMaterial.Name, material);

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
