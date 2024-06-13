using Space_Refinery_Engine;
using Space_Refinery_Engine.Audio;
using Space_Refinery_Utilities;
using Space_Refinery_Utilities.Units;
using System.Diagnostics;

namespace Tests
{
	[TestClass]
	public static class Initialize
	{ // TODO: was the FixedDecimal code flawless?
		[AssemblyInitialize]
		public static void AssemblyInitialize()
		{
			ReferenceHandler = new();

			ReferenceHandler.EnterAllowEventualReferenceMode(false);
			{
				ResourceDeserialization.DeserializeIntoGlobalReferenceHandler(ReferenceHandler, new(), out _, includeGameExtension: false);
			}
			ReferenceHandler.ExitAllowEventualReferenceMode();
		}

		public static SerializationReferenceHandler ReferenceHandler;
	}
}