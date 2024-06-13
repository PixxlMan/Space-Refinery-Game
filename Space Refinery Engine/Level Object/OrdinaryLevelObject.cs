using Space_Refinery_Engine;
using Space_Refinery_Utilities;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class OrdinaryLevelObject : LevelObject
{
	private OrdinaryLevelObject()
	{
		informationProvider = new LevelObjectInformationProvider(this);
	}
}
