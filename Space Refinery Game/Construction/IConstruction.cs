namespace Space_Refinery_Game
{
	public interface IConstruction : ISerializableReference, Entity
	{
		public void Deconstruct();

		public ConstructionInfo? ConstructionInfo { get; }
	}

	/*public interface IBuildableConstruction : IConstruction
	{
		public static abstract IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, GameData gameData, SerializationReferenceHandler referenceHandler);

		//public static abstract bool VerifyCompatibility(Connector connector);
	}*/
}