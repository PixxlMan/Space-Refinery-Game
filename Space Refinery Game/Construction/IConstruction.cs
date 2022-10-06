using FixedPrecision;
using Space_Refinery_Game_Renderer;
using System.Reflection;
using System.Xml;

namespace Space_Refinery_Game
{
	public interface IConstruction : ISerializableReference, Entity
	{
		public static abstract IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalLong8 rotation, GameData gameData, SerializationReferenceHandler referenceHandler);

		public void Deconstruct();

		public ConstructionInfo? ConstructionInfo { get; }

		//public static abstract bool VerifyCompatibility(Connector connector);
	}
}