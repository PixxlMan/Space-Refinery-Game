using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Space_Refinery_Engine;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using static Space_Refinery_Game.Renderer.RenderingResources;

namespace Space_Refinery_Game.Renderer;

public sealed class Material
{
	private readonly string name;
	private readonly ResourceSet textureSet;
	private readonly Texture albedoTexture;
	private readonly Texture normalTexture;
	private readonly Texture metallicTexture;
	private readonly Texture roughnessTexture;
	private readonly Texture ambientOcclusionTexture;

	private Material(string name, ResourceSet textureSet, Texture metallicTexture, Texture roughnessTexture, Texture ambientOcclusionTexture)
	{
		this.name = name;
		this.textureSet = textureSet;
		this.metallicTexture = metallicTexture;
		this.roughnessTexture = roughnessTexture;
		this.ambientOcclusionTexture = ambientOcclusionTexture;
	}

	public static Texture CreateTextureFromImage(Image<Rgba32> image, GraphicsDevice gd, ResourceFactory factory)
	{
		var imageTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)image.Width, (uint)image.Height, 1, 1u, PixelFormat.R8_G8_B8_A8_UNorm_SRgb, TextureUsage.Sampled));

		unsafe
		{
			if (!image.DangerousTryGetSinglePixelMemory(out var memory))
			{
				throw new Exception("ImageSharp memory was not contiguous and now the world is exploding.");
			}

			fixed (Rgba32* ptr = &MemoryMarshal.GetReference(memory.Span))
			{
				gd.UpdateTexture(texture: imageTexture, source: (IntPtr)ptr, sizeInBytes: (uint)(/*Unsafe.SizeOf<Rgba32>()*/ (sizeof(float) * 4) * image.Width * image.Height), x: 0u, y: 0u, z: 0u, width: (uint)image.Width, height: (uint)image.Height, depth: 1, mipLevel: 0, arrayLayer: 0);

				return imageTexture;
			}
		}
	}

	public static Texture CreateTextureFromBytes(ReadOnlySpan<byte> bytes, GraphicsDevice gd, ResourceFactory factory)
	{
		Image<Rgba32> image = Image.Load<Rgba32>(bytes);
		var imageTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)image.Width, (uint)image.Height, 1, 1u, PixelFormat.R8_G8_B8_A8_UNorm_SRgb, TextureUsage.Sampled));

		unsafe
		{
			if (!image.DangerousTryGetSinglePixelMemory(out var memory))
			{
				throw new Exception("ImageSharp memory was not contiguous and now the world is exploding.");
			}

			fixed (Rgba32* ptr = &MemoryMarshal.GetReference(memory.Span))
			{
				gd.UpdateTexture(texture: imageTexture, source: (IntPtr)ptr, sizeInBytes: (uint)(Unsafe.SizeOf<Rgba32>() * image.Width * image.Height), x: 0u, y: 0u, z: 0u, width: (uint)image.Width, height: (uint)image.Height, depth: 1, mipLevel: 0, arrayLayer: 0);

				return imageTexture;
			}
		}
	}

	public static Material LoadMaterial(string name, string albedoTexturePath, string normalTexturePath, string metallicTexturePath, string roughnessTexturePath, string ambientOcclusionTexturePath, GraphicsDevice gd, ResourceFactory factory)
	{
		Logging.LogScopeStart($"Loading resources for material {name}");

		Texture albedoTexture, normalTexture, metallicTexture, roughnessTexture, ambientOcclusionTexture;

		Logging.Log($"Loading albedo texture at {albedoTexturePath}");
		albedoTexture = CreateTextureFromImage(Image.Load<Rgba32>(albedoTexturePath), gd, factory);

		Logging.Log($"Loading normal texture at {normalTexturePath}");
		normalTexture = CreateTextureFromImage(Image.Load<Rgba32>(normalTexturePath), gd, factory);

		Logging.Log($"Loading metallic texture at {metallicTexturePath}");
		metallicTexture = CreateTextureFromImage(Image.Load<Rgba32>(metallicTexturePath), gd, factory);

		Logging.Log($"Loading roughness texture at {roughnessTexturePath}");
		roughnessTexture = CreateTextureFromImage(Image.Load<Rgba32>(roughnessTexturePath), gd, factory);

		Logging.Log($"Loading ambient occlusion texture at {ambientOcclusionTexturePath}");
		ambientOcclusionTexture = CreateTextureFromImage(Image.Load<Rgba32>(ambientOcclusionTexturePath), gd, factory);

		Material material = FromTextures(name, albedoTexture, normalTexture, metallicTexture, roughnessTexture, ambientOcclusionTexture, gd, factory);

		Logging.Log($"Created new material {name} from disk");

		Logging.LogScopeEnd();

		return material;
	}

	public static Material FromTextures(string name, Texture albedoTexture, Texture normalTexture, Texture metallicTexture, Texture roughnessTexture, Texture ambientOcclusionTexture, GraphicsDevice gd, ResourceFactory factory)
	{
		TextureView albedoTextureView = factory.CreateTextureView(albedoTexture);
		TextureView normalTextureView = factory.CreateTextureView(normalTexture);
		TextureView metallicTextureView = factory.CreateTextureView(metallicTexture);
		TextureView roughnessTextureView = factory.CreateTextureView(roughnessTexture);
		TextureView ambientOcclusionTextureView = factory.CreateTextureView(ambientOcclusionTexture);
		
		var textureSet = factory.CreateResourceSet(
			new ResourceSetDescription(
				MaterialLayout,
				gd.Aniso4xSampler,
				albedoTextureView,
				normalTextureView,
				metallicTextureView,
				roughnessTextureView,
				ambientOcclusionTextureView
			)
		);

		return new Material(name, textureSet, metallicTexture, roughnessTexture, ambientOcclusionTexture);
	}

	public void AddSetCommands(CommandList cl)
	{
		cl.SetGraphicsResourceSet(1, textureSet);
	}
}
