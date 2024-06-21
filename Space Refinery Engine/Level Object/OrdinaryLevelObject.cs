namespace Space_Refinery_Engine;

public sealed class OrdinaryLevelObject : StaticLevelObject
{
	private OrdinaryLevelObject()
	{
		informationProvider = new LevelObjectInformationProvider(this);
	}
}
