using Space_Refinery_Game_Renderer;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed record class MaterialInfo
	: ISerializableReference
{
	public SerializableReference SerializableReference { get; private set; }

	public MaterialLoadingDescription MaterialTexturePaths { get; private set; }

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(this);

		writer.Serialize(MaterialTexturePaths.DiffuseTexturePath, nameof(MaterialTexturePaths.DiffuseTexturePath));
		writer.Serialize(MaterialTexturePaths.MetallicTexturePath, nameof(MaterialTexturePaths.MetallicTexturePath));
		writer.Serialize(MaterialTexturePaths.RoughnessTexturePath, nameof(MaterialTexturePaths.RoughnessTexturePath));
		writer.Serialize(MaterialTexturePaths.AmbientOcclusionTexturePath, nameof(MaterialTexturePaths.AmbientOcclusionTexturePath));
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		SerializableReference = reader.ReadReference();

		var diffuseTexturePath = reader.ReadResorucePath(serializationData, nameof(MaterialTexturePaths.DiffuseTexturePath));
		var metallicTexturePath = reader.ReadResorucePath(serializationData, nameof(MaterialTexturePaths.MetallicTexturePath));
		var roughnessTexturePath = reader.ReadResorucePath(serializationData, nameof(MaterialTexturePaths.RoughnessTexturePath));
		var ambientOcclusionTexturePath = reader.ReadResorucePath(serializationData, nameof(MaterialTexturePaths.AmbientOcclusionTexturePath));

		MaterialTexturePaths = new(SerializableReference.ToString(), diffuseTexturePath, metallicTexturePath, roughnessTexturePath, ambientOcclusionTexturePath);
	}
}
