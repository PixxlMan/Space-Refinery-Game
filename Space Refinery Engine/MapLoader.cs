using FixedPrecision;
using SharpGLTF.Scenes;
using Space_Refinery_Game.Renderer;
using System.Numerics;
using System.Xml;
using Veldrid.Utilities;

namespace Space_Refinery_Engine;

public static class MapLoader
{
	public static void LoadMap(MapInfo mapInfo, SerializationReferenceHandler referenceHandler, GameData gameData)
	{
		Logging.LogScopeStart($"Loading map '{mapInfo.SerializableReference}' at {mapInfo.MapPath}");

		var scene = SceneBuilder.LoadDefaultScene(mapInfo.MapPath);

		foreach (var instance in scene.Instances)
		{
			string name = instance.Name.Split('.')[0];

			Logging.LogScopeStart($"Creating '{name}'");

			var gltfTransform = ((RigidTransformer)instance.Content).Transform;

			QuaternionFixedDecimalInt4 rotation;
			if (gltfTransform.Rotation is null)
			{
				rotation = QuaternionFixedDecimalInt4.Identity;
			}
			else
			{
				rotation = gltfTransform.Rotation.Value.ToFixed<QuaternionFixedDecimalInt4>();
			}

			Transform transform = new(gltfTransform.Translation.Value.ToFixed<Vector3FixedDecimalInt4>(), rotation);
			Logging.Log(transform.ToString()!);

			Vector3 scale;
			if (gltfTransform.Scale is null)
			{
				scale = Vector3.One;
			}
			else
			{
				scale = gltfTransform.Scale.Value;
			}
			Logging.Log($"Scale = {scale}");

			var meshInfo = instance.Content.GetGeometryAsset().Primitives.First();

			if (!gameData.GraphicsWorld.MeshLoader.TryGetCached(name, out var mesh))
			{
				var verticies = new VertexPositionNormalTexture[meshInfo.Vertices.Count];

				for (int i = 0; i < meshInfo.Vertices.Count; i++)
				{
					SharpGLTF.Geometry.IVertexBuilder? vertexBuilder = meshInfo.Vertices[i];
					var geometry = vertexBuilder.GetGeometry();

					var position = geometry.GetPosition() * scale;
					geometry.TryGetNormal(out var normal);
					var texCoords = vertexBuilder.GetMaterial().GetTexCoord(0);

					verticies[i] = new(position, normal, texCoords);
				}

				mesh = Mesh.CreateMesh(meshInfo.GetIndices().Select((i) => (ushort)i).ToArray(), verticies, Veldrid.FrontFace.CounterClockwise, gameData.GraphicsWorld.GraphicsDevice, gameData.GraphicsWorld.Factory);
				gameData.GraphicsWorld.MeshLoader.AddCache(name, mesh);
			}

			LevelObjectType levelObjectType;
			var levelObjectTypeNameJsonNode = instance.Extras?["LevelObjectType"];
			if (levelObjectTypeNameJsonNode is null)
			{
				if (LevelObjectType.LevelObjectTypes.TryGetValue(name, out LevelObjectType? value))
				{
					levelObjectType = value;
				}
				else
				{
					levelObjectType = new(name, mesh!, new Collider(ColliderShapes.ConvexMesh, Transform.Identity, mesh: mesh), gameData.GraphicsWorld.MaterialLoader.LoadGLTFMaterial(meshInfo.Material), typeof(OrdinaryLevelObject));

					levelObjectType.SetUp(gameData);
				}
			}
			else
			{
				var levelObjectTypeName = levelObjectTypeNameJsonNode.GetValue<string>();
				if (LevelObjectType.LevelObjectTypes.TryGetValue(levelObjectTypeName, out LevelObjectType? value))
				{
					levelObjectType = value;
				}
				else
				{
					throw new Exception($"No {nameof(LevelObjectType)} exists with the name '{levelObjectTypeName}'");
				}
			}

			Logging.Log($"LevelObjectType = {levelObjectType.Name}");

			LevelObject.Create(levelObjectType, transform, gameData, referenceHandler);

			Logging.LogScopeEnd();
		}

		Logging.LogScopeEnd();
	}
}

public sealed class MapInfo : ISerializableReference
{
	public SerializableReference serializableReference;
	public SerializableReference SerializableReference => serializableReference;

	public string MapPath { get; private set; }

	private MapInfo()
	{ }

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		serializableReference = reader.ReadReference();

		MapPath = Path.Combine(serializationData.BasePathForAssetDeserialization!, reader.ReadString(nameof(MapPath)));
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(this);

		writer.Serialize(MapPath, nameof(MapPath));
	}
}