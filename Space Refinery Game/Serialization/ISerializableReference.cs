namespace Space_Refinery_Game
{
	public interface ISerializableReference : IEntitySerializable
	{
		public SerializableReference SerializableReference { get; }
	}
}