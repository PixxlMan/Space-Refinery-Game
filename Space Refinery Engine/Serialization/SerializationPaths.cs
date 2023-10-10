namespace Space_Refinery_Engine;

public static class SerializationPaths
{
	public static readonly string AssetsPath = Path.Combine(Environment.CurrentDirectory, "Assets");

	public static readonly string ModPath = Path.Combine(Environment.CurrentDirectory, "Mods");

	public static readonly string ExtensionManifestFileExtension = ".manifest.srh.xml";

	public static readonly string SerializableReferenceHandlerFileExtension = ".srh.xml";
}
