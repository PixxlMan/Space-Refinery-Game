using FixedPrecision;
using SharpGLTF.Scenes;
using Space_Refinery_Engine.Renderer;
using System.Numerics;
using System.Xml;

namespace Space_Refinery_Engine;

public static class MapLoader
{
	public static void LoadMap(MapInfo mapInfo, SerializationReferenceHandler referenceHandler, GameData gameData)
	{
		Logging.LogScopeStart($"Loading map '{mapInfo.SerializableReference}' at {mapInfo.MapPath}");

		var scene = SceneBuilder.LoadDefaultScene(mapInfo.MapPath);

		gameData.GraphicsWorld.MeshLoader.LoadAndCacheAll(scene);

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

			LevelObjectType levelObjectType;
			var levelObjectTypeNameJsonNode = instance.Extras?["LevelObjectType"];
			if (levelObjectTypeNameJsonNode is null)
			{
				if (gltfTransform.Scale is not null && gltfTransform.Scale.Value != Vector3.One)
				{
					throw new NotSupportedException("LevelObjects with an embedded mesh with a non-identity scale are not supported");
				}

				if (LevelObjectType.LevelObjectTypes.TryGetValue(name, out LevelObjectType? value))
				{
					levelObjectType = value;
				}
				else
				{
					if (!gameData.GraphicsWorld.MeshLoader.TryGetCached(name, out Mesh? mesh))
					{
						throw new GlitchInTheMatrixException($"Requested mesh {name} was not found in cache");
					}

					levelObjectType = new(name, mesh, new Collider(ColliderShapes.ConvexMesh, Transform.Identity, mesh: mesh), gameData.GraphicsWorld.MaterialLoader.LoadGLTFMaterial(instance.Content.GetGeometryAsset().Primitives.First().Material), typeof(OrdinaryLevelObject));

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