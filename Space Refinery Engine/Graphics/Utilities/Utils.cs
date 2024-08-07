﻿using FixedPrecision;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace Space_Refinery_Engine.Renderer;

public static class Utils
{
	public static Shader LoadShader(ResourceFactory factory, string path, ShaderStages stage, string entryPoint)
	{
		string name = $"{path}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}";
		return factory.CreateShader(new ShaderDescription(stage, ReadEmbeddedAssetBytes(name), entryPoint));
	}

	public static byte[] ReadEmbeddedAssetBytes(string path)
	{
		using (Stream stream = File.OpenRead(path))
		{
			byte[] bytes = new byte[stream.Length];
			using (MemoryStream ms = new(bytes))
			{
				stream.CopyTo(ms);
				return bytes;
			}
		}
	}

	private static string GetExtension(GraphicsBackend backendType)
	{
		bool isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");

		return (backendType == GraphicsBackend.Direct3D11)
			? "hlsl.bytes"
			: (backendType == GraphicsBackend.Vulkan)
				? "450.glsl.spv"
				: (backendType == GraphicsBackend.Metal)
					? isMacOS ? "metallib" : "ios.metallib"
					: (backendType == GraphicsBackend.OpenGL)
						? "330.glsl"
						: "300.glsles";
	}

	public static Shader[] LoadShaders(string path, string shaderSetName, ResourceFactory factory)
	{
		return factory.CreateFromSpirv(
			new ShaderDescription(ShaderStages.Vertex, ReadBytes(Path.Combine(path, shaderSetName) + "-vertex.glsl"), "main"),
			new ShaderDescription(ShaderStages.Fragment, ReadBytes(Path.Combine(path, shaderSetName) + "-fragment.glsl"), "main"));
	}

	public static Shader LoadShader(string path, string shaderName, ResourceFactory factory)
	{
		return factory.CreateFromSpirv(
			new ShaderDescription(ShaderStages.Compute, ReadBytes(Path.Combine(path, shaderName) + "-compute.glsl"), "main"));
	}

	public static byte[] ReadBytes(string path)
	{
		using (Stream stream = File.OpenRead(path))
		{
			byte[] bytes = new byte[stream.Length];
			using (MemoryStream ms = new(bytes))
			{
				stream.CopyTo(ms);
				return bytes;
			}
		}
	}

	public static unsafe Texture GetSolidColoredTexture(RgbaByte color, GraphicsDevice gd, ResourceFactory factory)
	{
		var pinkTexture = factory.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
		gd.UpdateTexture(pinkTexture, (IntPtr)(&color), 4, 0, 0, 0, 1, 1, 1, 0, 0);

		return pinkTexture;
	}

	public static Memory<VertexPositionTexture2D> GetQuadVertexPositionTexture(bool isClipSpaceYInverted = false)
	{
		if (isClipSpaceYInverted)
		{
			return new Memory<VertexPositionTexture2D>
				([
					new(new(-1, -1), new(0, 0)),
					new(new(1, -1), new(1, 0)),
					new(new(1, 1), new(1, 1)),
					new(new(-1, 1), new(0, 1))
				]);
		}
		else
		{
			return new Memory<VertexPositionTexture2D>
			([
				new(new(-1, 1), new(0, 0)),
				new(new(1, 1), new(1, 0)),
				new(new(1, -1), new(1, 1)),
				new(new(-1, -1), new(0, 1))
			]);
		}
	}

	public static VertexPositionTexture[] GetCubeVertexPositionTexture(Vector3 scale)
	{
		VertexPositionTexture[] vertices =
		[
			// Top
			new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 1)),
			// Bottom                                                             
			new VertexPositionTexture(new Vector3(-0.5f,-0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(+0.5f,-0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(+0.5f,-0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(-0.5f,-0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 1)),
			// Left                                                               
			new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 1)),
			// Right                                                              
			new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 1)),
			// Back                                                               
			new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 1)),
			// Front                                                              
			new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f) * scale, new Vector3(0.5f, 0.5f, 1f), new Vector2(0, 1)),
		];

		return vertices;
	}

	public static ushort[] GetCubeIndices()
	{
		ushort[] indices =
		{
			0,1,2, 0,2,3,
			4,5,6, 4,6,7,
			8,9,10, 8,10,11,
			12,13,14, 12,14,15,
			16,17,18, 16,18,19,
			20,21,22, 20,22,23,
		};

		return indices;
	}

	public static ModelResources CreateDeviceResourcesModelResources(VertexPositionTexture[] vertexData, ushort[] indexData, GraphicsDevice gd, ResourceFactory factory)
	{
		DeviceBuffer vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionTexture.SizeInBytes * vertexData.Length), BufferUsage.VertexBuffer));
		gd.UpdateBuffer(vertexBuffer, 0, vertexData);

		DeviceBuffer indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(ushort) * (uint)indexData.Length, BufferUsage.IndexBuffer));
		gd.UpdateBuffer(indexBuffer, 0, indexData);

		return new ModelResources(vertexBuffer, indexBuffer, IndexFormat.UInt16, (uint)indexData.Length);
	}

	public struct ModelResources
	{
		public readonly DeviceBuffer VertexBuffer;
		public readonly DeviceBuffer IndexBuffer;
		public readonly IndexFormat IndexFormat;
		public readonly uint IndexCount;

		public ModelResources(DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, IndexFormat indexFormat, uint indexCount)
		{
			VertexBuffer = vertexBuffer;
			IndexBuffer = indexBuffer;
			IndexFormat = indexFormat;
			IndexCount = indexCount;
		}
	}

	public static Texture CreateIdenticalTexture(Texture texture, ResourceFactory factory)
	{
		return factory.CreateTexture(
			TextureDescription.Texture2D(
				texture.Width,
				texture.Height,
				texture.MipLevels,
				texture.ArrayLayers,
				texture.Format,
				texture.Usage
				)
			);
	}
	
	public static Texture CloneTexture(Texture texture, CommandList commandList, ResourceFactory factory)
	{
		var clone = CreateIdenticalTexture(texture, factory);
		commandList.CopyTexture(texture, clone);

		return clone;
	}

	public static Texture CloneTexture(Texture texture, GraphicsDevice graphicsDevice, ResourceFactory factory)
	{
		var commandList = factory.CreateCommandList();
		commandList.Begin();

		var clone = CloneTexture(texture, commandList, factory);

		var fence = factory.CreateFence(false);
		fence.Name = "Texture clone completion fence";

		commandList.End();
		graphicsDevice.SubmitCommands(commandList, fence);
		graphicsDevice.WaitForFence(fence);
		commandList.Dispose();
		fence.Dispose();

		return clone;
	}

	public static Mesh CreateNormallessTangentlessMesh(string name, VertexPositionTexture[] vertexData, ushort[] indexData, GraphicsDevice gd, ResourceFactory factory)
	{
		DeviceBuffer vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionTexture.SizeInBytes * vertexData.Length), BufferUsage.VertexBuffer));
		gd.UpdateBuffer(vertexBuffer, 0, vertexData);

		DeviceBuffer indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(ushort) * (uint)indexData.Length, BufferUsage.IndexBuffer));
		gd.UpdateBuffer(indexBuffer, 0, indexData);

		return new Mesh(name, vertexBuffer, indexBuffer, IndexFormat.UInt16, (uint)indexData.Length);
	}

	public static Mesh GetCubeMesh(Vector3FixedDecimalInt4 size, GraphicsDevice graphicsDevice, ResourceFactory factory)
	{
		Mesh mesh;
		
		mesh = CreateNormallessTangentlessMesh($"{size.ToString()} cube mesh", GetCubeVertexPositionTexture(size.ToVector3()), GetCubeIndices(), graphicsDevice, factory);

		return mesh;
	}

	public static Mesh GetQuadMesh(Vector2FixedDecimalInt4 size, GraphicsDevice graphicsDevice, ResourceFactory factory)
	{
		Mesh mesh;

		mesh = CreateNormallessTangentlessMesh($"{size.ToString()} quad mesh", GetQuadVertexPositionTexture(size.ToVector2()), GetQuadIndices(), graphicsDevice, factory);

		return mesh;
	}

	private static VertexPositionTexture[] GetQuadVertexPositionTexture(Vector2 size)
	{
		VertexPositionTexture[] vertices =
		[
			// Front                   
			new VertexPositionTexture(new Vector3(new Vector2(-.5f, +.5f) * size, 0), new Vector3(0, 0, 1f), new Vector2(0, 0)),
			new VertexPositionTexture(new Vector3(new Vector2(+.5f, +.5f) * size, 0), new Vector3(0, 0, 1f), new Vector2(1, 0)),
			new VertexPositionTexture(new Vector3(new Vector2(+.5f, -.5f) * size, 0), new Vector3(0, 0, 1f), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(new Vector2(-.5f, -.5f) * size, 0), new Vector3(0, 0, 1f), new Vector2(0, 1)),
		];

		return vertices;
	}

	public static ushort[] GetQuadIndices()
	{
		ushort[] indices =
		{
			0,1,2, 0,2,3,
		};

		return indices;
	}
}

public struct VertexPositionTexture
{
	public const uint SizeInBytes = sizeof(float) * 8;

	public float PosX;
	public float PosY;
	public float PosZ;

	public float NormalX;
	public float NormalY;
	public float NormalZ;

	public float TexU;
	public float TexV;

	public VertexPositionTexture(Vector3 pos, Vector3 normal, Vector2 uv)
	{
		PosX = pos.X;
		PosY = pos.Y;
		PosZ = pos.Z;

		NormalX = normal.X;
		NormalY = normal.Y;
		NormalZ = normal.Z;

		TexU = uv.X;
		TexV = uv.Y;
	}
}

public struct VertexPositionTexture2D
{
	public const uint SizeInBytes = sizeof(float) * 4;

	public float PosX;
	public float PosY;

	public float TexU;
	public float TexV;

	public VertexPositionTexture2D(Vector2 pos, Vector2 uv)
	{
		PosX = pos.X;
		PosY = pos.Y;

		TexU = uv.X;
		TexV = uv.Y;
	}
}
