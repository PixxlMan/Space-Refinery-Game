using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Xml;
using Veldrid.Utilities;

namespace Space_Refinery_Engine;

public static class MapLoader
{
	public static void LoadMap(MapInfo mapInfo, SerializationReferenceHandler referenceHandler, GameData gameData)
	{
		Logging.LogScopeStart($"Loading map '{mapInfo.SerializableReference}' at {mapInfo.MapPath}");

		ObjParser objParser = new();

		ObjFile objFile = objParser.Parse(File.ReadAllLines(mapInfo.MapPath));

		foreach (var meshGroup in objFile.MeshGroups)
		{
			Logging.LogScopeStart($"Creating '{meshGroup.Name}'");

			ConstructedMeshInfo meshInfo = objFile.GetMesh(meshGroup);
			Transform transform = new(Vector3FixedDecimalInt4.Zero, QuaternionFixedDecimalInt4.Identity);

			Logging.Log(transform.ToString()!);

			if (!gameData.GraphicsWorld.MeshLoader.TryGetCached(meshGroup.Name, out var mesh))
			{
				mesh = Mesh.CreateMesh(meshInfo.GetIndices(), meshInfo.Vertices, gameData.GraphicsWorld.GraphicsDevice, gameData.GraphicsWorld.Factory);
				gameData.GraphicsWorld.MeshLoader.AddCache(meshGroup.Name, mesh);
			}

			LevelObjectType levelObjectType;
			if (meshGroup.Material != "(null)")
			{
				levelObjectType = (LevelObjectType)referenceHandler[meshGroup.Material];
			}
			else
			{
				if (LevelObjectType.LevelObjectTypes.TryGetValue(meshGroup.Material, out LevelObjectType? value))
				{
					levelObjectType = value;
				}
				else
				{
					levelObjectType = new(meshGroup.Name, mesh!, gameData.GraphicsWorld.MaterialLoader.LoadCached(((MaterialInfo)referenceHandler["Rusty Metal Sheet"]).MaterialTexturePaths), typeof(OrdinaryLevelObject));

					levelObjectType.SetUp(gameData);
				}
			}

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