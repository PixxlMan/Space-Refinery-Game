namespace Space_Refinery_Engine;

public sealed class OrdinaryLevelObject : LevelObject
{
	private OrdinaryLevelObject()
	{
		informationProvider = new LevelObjectInformationProvider(this);
	}
}
