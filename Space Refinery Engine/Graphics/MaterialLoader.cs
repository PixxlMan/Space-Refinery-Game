using SharpGLTF.Materials;
using SharpGLTF.Memory;
using Space_Refinery_Engine;
using Veldrid;

namespace Space_Refinery_Game.Renderer;

public sealed class MaterialLoader(GraphicsWorld graphicsWorld)
{
	private Dictionary<string, Material> materialCache = [];

	private GraphicsWorld graphicsWorld = graphicsWorld;

	public Material LoadCached(MaterialLoadingDescription materialTexturePaths)
	{
		if (materialCache.TryGetValue(materialTexturePaths.Name, out Material? material))
		{
			return material;
		}

		material = Material.LoadMaterial(
			materialTexturePaths.Name,
			materialTexturePaths.AlbedoTexturePath,
			materialTexturePaths.NormalTexturePath,
			materialTexturePaths.MetallicTexturePath,
			materialTexturePaths.RoughnessTexturePath,
			materialTexturePaths.AmbientOcclusionTexturePath,
			graphicsWorld.GraphicsDevice,
			graphicsWorld.Factory
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

		Logging.LogScopeStart($"Loading resources for material {gltfMaterial.Name} from GLTF material");

		var albedoImage = gltfMaterial.GetChannel(KnownChannel.BaseColor).GetValidTexture().PrimaryImage.Content;
		Texture normalTexture;
		if (gltfMaterial.GetChannel(KnownChannel.Normal) is not null)
		{
			normalTexture = Material.CreateTextureFromBytes(gltfMaterial.GetChannel(KnownChannel.Normal).GetValidTexture().PrimaryImage.Content.Content.Span, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
		}
		else
		{
			normalTexture = RenderingResources.NeutralNormal;
		}
		var metallicImage = gltfMaterial.GetChannel(KnownChannel.MetallicRoughness).GetValidTexture().PrimaryImage.Content;
		var roughnessImage = gltfMaterial.GetChannel(KnownChannel.MetallicRoughness).GetValidTexture().PrimaryImage.Content;
		Texture aoTexture;
		if (gltfMaterial.GetChannel(KnownChannel.Occlusion) is not null)
		{
			aoTexture = Material.CreateTextureFromBytes(gltfMaterial.GetChannel(KnownChannel.Occlusion).GetValidTexture().PrimaryImage.Content.Content.Span, graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
		}
		else
		{
			aoTexture = RenderingResources.WhiteTexture;
		}

		material = Material.FromTextures(
			gltfMaterial.Name,
			Material.CreateTextureFromBytes(albedoImage.Content.Span, graphicsWorld.GraphicsDevice, graphicsWorld.Factory),
			normalTexture,
			Material.CreateTextureFromBytes(metallicImage.Content.Span, graphicsWorld.GraphicsDevice, graphicsWorld.Factory),
			Material.CreateTextureFromBytes(roughnessImage.Content.Span, graphicsWorld.GraphicsDevice, graphicsWorld.Factory),
			aoTexture,
			graphicsWorld.GraphicsDevice,
			graphicsWorld.Factory
			);

		materialCache.Add(gltfMaterial.Name, material);

		Logging.LogScopeEnd();
		return material;
	}
}

public record struct MaterialLoadingDescription
	(
	string Name,
	string AlbedoTexturePath,
	string NormalTexturePath,
	string MetallicTexturePath,
	string RoughnessTexturePath,
	string AmbientOcclusionTexturePath
	)
{

}
