using System.Xml;

namespace Space_Refinery_Game
{
	public abstract class Setting : ISerializableReference
	{
		public abstract ISettingValue SettingValue { get; set; }

		public abstract bool Dirty { get; }

		public SerializableReference SerializableReference { get; protected set; }

		public ISettingValue DefaultValue { get; protected set; }

		public string Name { get; private set; }

		public abstract void DoUI();

		public abstract void Accept();

		public abstract void Cancel();

		public abstract void ValueChanged();

		public virtual void SerializeState(XmlWriter writer)
		{
			writer.SerializeReference(this);
			writer.Serialize(Name, nameof(Name));
			writer.SerializeWithEmbeddedType(DefaultValue, nameof(DefaultValue));
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			SerializableReference = reader.ReadReference();
			Name = reader.ReadString(nameof(Name));
			DefaultValue = (ISettingValue)reader.DeserializeEntitySerializableWithEmbeddedType(serializationData, referenceHandler, nameof(DefaultValue));
			SettingValue = DefaultValue;

			serializationData.GameData.Settings.AddSetting(this);
		}

		public void SetDefault()
		{
			SettingValue = DefaultValue;
			ValueChanged();
		}

		public abstract event Action<ISettingValue> AcceptedSettingChange;

		public abstract event Action<ISettingValue> SettingChanged;
	}

	public interface ISettingValue : IEntitySerializable
	{

	}
}
