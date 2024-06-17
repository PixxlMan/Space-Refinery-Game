using Space_Refinery_Game_Renderer;
using System.Collections.Concurrent;
using System.Xml;

namespace Space_Refinery_Engine
{
	public sealed class LevelObjectType : IEntityType, ISerializableReference
	{
		public static ConcurrentDictionary<string, LevelObjectType> LevelObjectTypes = new();

		public string ModelPath { get; private set; }

		public Mesh Mesh { get; private set; }

		public MaterialInfo MaterialInfo { get; private set; }

		public Material Material { get; private set; }

		public Type TypeOfLevelObject { get; private set; }

		public Collider Collider { get; private set; }

		public BatchRenderable BatchRenderable { get; private set; }

		public SerializableReference SerializableReference { get; private set; }

		public string Name { get; private set; }

		public LevelObjectType(string name, string modelPath, Mesh mesh, Type typeOfPipe)
		{
			Name = name;
			ModelPath = modelPath;
			Mesh = mesh;
			TypeOfLevelObject = typeOfPipe;

			SerializableReference = Guid.NewGuid();

			if (!LevelObjectTypes.TryAdd(Name, this))
			{
				throw new Exception($"Couldn't add {nameof(LevelObject)} '{Name}' to dictionary of all available LevelObjectTypes as another pipe type with the same name already exists.");
			}
		}

		private LevelObjectType()
		{

		}

		public void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);

			writer.WriteElementString(nameof(Name), Name);

			writer.WriteElementString(nameof(ModelPath), ModelPath);

			writer.SerializeReference(MaterialInfo, nameof(MaterialInfo));

			writer.Serialize(TypeOfLevelObject);
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SerializableReference = reader.ReadReference();

			Name = reader.ReadString(nameof(Name));

			ModelPath = reader.ReadResorucePath(serializationData, nameof(ModelPath));

			Mesh = serializationData.GameData.GraphicsWorld.MeshLoader.LoadCached(ModelPath);

			IEntitySerializable.DeserializeWithoutEmbeddedType<Collider>(reader, serializationData, referenceHandler, nameof(Collider));

			reader.DeserializeReference<MaterialInfo>(referenceHandler, (mI) => MaterialInfo = mI, nameof(MaterialInfo));

			TypeOfLevelObject = reader.DeserializeSerializableType();

			if (!LevelObjectTypes.TryAdd(Name, this))
			{
				throw new Exception($"Couldn't add {nameof(LevelObject)} '{Name}' to dictionary of all available LevelObjectTypes as another pipe type with the same name already exists.");
			}

			serializationData.DeserializationCompleteEvent += () =>
			{
				BatchRenderable = BatchRenderable.CreateAndAdd($"{Name} LevelObject Type Batch Renderable", serializationData.GameData.GraphicsWorld, Mesh, serializationData.GameData.GraphicsWorld.MaterialLoader.LoadCached(MaterialInfo.MaterialTexturePaths), serializationData.GameData.GraphicsWorld.CameraProjViewBuffer, serializationData.GameData.GraphicsWorld.LightInfoBuffer);

				serializationData.GameData.GraphicsWorld.AddRenderable(BatchRenderable);
			};
		}
	}
}
