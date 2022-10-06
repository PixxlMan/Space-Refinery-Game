namespace Space_Refinery_Game
{
	public interface ISerializableReference : IEntitySerializable
	{
		public Guid SerializableReferenceGUID { get; }
	}
}