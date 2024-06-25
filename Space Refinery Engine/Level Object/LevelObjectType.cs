using Space_Refinery_Game.Renderer;
using System.Collections.Concurrent;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class LevelObjectType : IEntityType, ISerializableReference
{
	public static ConcurrentDictionary<string, LevelObjectType> LevelObjectTypes { get; private set; } = new();

	public string? ModelPath { get; private set; }

	public Mesh Mesh { get; private set; }

	public MaterialInfo? MaterialInfo { get; private set; }

	public Material? Material { get; private set; }

	public Type TypeOfLevelObject { get; private set; }

	public Collider Collider { get; private set; }

	public BatchRenderable BatchRenderable { get; private set; }

	public SerializableReference SerializableReference { get; private set; }

	public string Name { get; private set; }

	public LevelObjectType(string name, string modelPath, Mesh mesh, Collider collider, Type typeOfLevelObject)
	{
		Name = name;
		ModelPath = modelPath;
		Mesh = mesh;
		Collider = collider;
		TypeOfLevelObject = typeOfLevelObject;

		SerializableReference = Guid.NewGuid();
	}

	public LevelObjectType(string name, Mesh mesh, Collider collider, Material material, Type typeOfLevelObject)
	{
		Name = name;
		Mesh = mesh;
		Collider = collider;
		Material = material;
		TypeOfLevelObject = typeOfLevelObject;

		SerializableReference = Guid.NewGuid();
	}

	private LevelObjectType()
	{

	}

	public void SetUp(GameData gameData)
	{
		Material ??= gameData.GraphicsWorld.MaterialLoader.LoadCached(MaterialInfo.MaterialTexturePaths);

		BatchRenderable = BatchRenderable.CreateAndAdd($"{Name} LevelObject Type Batch Renderable", gameData.GraphicsWorld, Mesh, Material, gameData.GraphicsWorld.CameraProjViewBuffer, gameData.GraphicsWorld.LightInfoBuffer);

		gameData.GraphicsWorld.AddRenderable(BatchRenderable);

		if (!LevelObjectTypes.TryAdd(Name, this))
		{
			throw new Exception($"Couldn't add {nameof(LevelObject)} '{Name}' to dictionary of all available LevelObjectTypes as another LevelObject with the same name already exists.");
		}
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(this);

		writer.WriteElementString(nameof(Name), Name);

		writer.WriteElementString(nameof(ModelPath), ModelPath);

		writer.SerializeWithoutEmbeddedType(Collider, nameof(Collider));

		writer.SerializeReference(MaterialInfo, nameof(MaterialInfo));

		writer.Serialize(TypeOfLevelObject);
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		SerializableReference = reader.ReadReference();

		Name = reader.ReadString(nameof(Name));

		ModelPath = reader.ReadResorucePath(serializationData, nameof(ModelPath));

		Mesh = serializationData.GameData.GraphicsWorld.MeshLoader.LoadCached(ModelPath);

		Collider = reader.DeserializeEntitySerializableWithoutEmbeddedType<Collider>(serializationData, referenceHandler, nameof(Collider));

		reader.DeserializeReference<MaterialInfo>(referenceHandler, (mI) => MaterialInfo = mI, nameof(MaterialInfo));

		TypeOfLevelObject = reader.DeserializeSerializableType();

		serializationData.DeserializationCompleteEvent += () =>
		{
			SetUp(serializationData.GameData);
		};
	}
}
