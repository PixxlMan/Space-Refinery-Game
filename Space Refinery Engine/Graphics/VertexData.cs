using System.Numerics;

namespace Space_Refinery_Engine.Renderer;

public struct VertexData
{
	public const byte SizeInBytes = 48;
	public const byte NormalOffset = 12;
	public const byte TextureCoordinatesOffset = 24;
	public const byte TangentOffset = 32;
	public const byte ElementCount = 4;

	public readonly Vector3 Position;
	public readonly Vector3 Normal;
	public readonly Vector2 TextureCoordinates;
	public readonly Vector3 Tangent;

	public VertexData(Vector3 position, Vector3 normal, Vector2 texCoords, Vector3 tangent)
	{
		Position = position;
		Normal = normal;
		TextureCoordinates = texCoords;
		Tangent = tangent;
	}
}