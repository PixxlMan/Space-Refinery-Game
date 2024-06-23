using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Space_Refinery_Utilities;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using static Space_Refinery_Game_Renderer.RenderingResources;

namespace Space_Refinery_Game_Renderer;

public sealed class Material
{
	private readonly string name;
	private readonly ResourceSet textureSet;
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

	public static Texture CreateTextureFromImage(GraphicsDevice gd, ResourceFactory factory, Image<Rgba32> image)
	{
		var imageTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)image.Width, (uint)image.Height, 1, 1u, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

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

	public static Texture CreateTextureFromBytes(GraphicsDevice gd, ResourceFactory factory, ReadOnlySpan<byte> bytes)
	{
		Image<Rgba32> image = Image.Load<Rgba32>(bytes);
		var imageTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)image.Width, (uint)image.Height, 1, 1u, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

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

	public static Material LoadMaterial(GraphicsDevice gd, ResourceFactory factory, string name, string diffuseTexturePath, string metallicTexturePath, string roughnessTexturePath, string ambientOcclusionTexturePath)
	{
		Logging.LogScopeStart($"Loading resources for material {name}");

		Texture diffuseTexture, metallicTexture, roughnessTexture, ambientOcclusionTexture;

		Logging.Log($"Loading diffuse texture at {diffuseTexturePath}");
		diffuseTexture = CreateTextureFromImage(gd, factory, Image<Rgba32>.Load<Rgba32>(diffuseTexturePath));

		Logging.Log($"Loading metallic texture at {metallicTexturePath}");
		metallicTexture = CreateTextureFromImage(gd, factory, Image<Rgba32>.Load<Rgba32>(metallicTexturePath));

		Logging.Log($"Loading roughness texture at {roughnessTexturePath}");
		roughnessTexture = CreateTextureFromImage(gd, factory, Image<Rgba32>.Load<Rgba32>(roughnessTexturePath));

		Logging.Log($"Loading ambient occlusion texture at {ambientOcclusionTexturePath}");
		ambientOcclusionTexture = CreateTextureFromImage(gd, factory, Image<Rgba32>.Load<Rgba32>(ambientOcclusionTexturePath));

		Material material = FromTextures(gd, factory, name, diffuseTexture, metallicTexture, roughnessTexture, ambientOcclusionTexture);

		Logging.LogScopeEnd();

		return material;
	}

	public static Material FromTextures(GraphicsDevice gd, ResourceFactory factory, string name, Texture diffuseTexture, Texture metallicTexture, Texture roughnessTexture, Texture ambientOcclusionTexture)
	{
		TextureView diffuseTextureView = factory.CreateTextureView(diffuseTexture);
		TextureView metallicTextureView = factory.CreateTextureView(metallicTexture);
		TextureView roughnessTextureView = factory.CreateTextureView(roughnessTexture);
		TextureView ambientOcclusionTextureView = factory.CreateTextureView(ambientOcclusionTexture);
		
		var textureSet = factory.CreateResourceSet(
			new ResourceSetDescription(
				MaterialLayout,
				gd.Aniso4xSampler,
				diffuseTextureView,
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
