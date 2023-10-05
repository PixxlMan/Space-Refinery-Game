namespace Space_Refinery_Engine
{
	public interface ISerializableReference : IEntitySerializable
	{
		public SerializableReference SerializableReference { get; }
	}
}