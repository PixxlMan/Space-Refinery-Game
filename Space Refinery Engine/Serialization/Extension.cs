using System.Reflection;

namespace Space_Refinery_Engine;

public sealed class Extension(string extensionName, bool hasAssembly, Assembly? hostAssembly, ExtensionManifest extensionManifest)
{
	public string ExtensionName = extensionName;

	public bool HasAssembly = hasAssembly;

	public Assembly? HostAssembly = hostAssembly;

	public ExtensionManifest ExtensionManifest = extensionManifest;

	public static Extension CreateAndLoadFromExtensionManifest(ExtensionManifest extensionManifest, string extensionDirectoryName)
	{
		if (extensionManifest.HasAssembly)
		{
			Assembly hostAssembly = Assembly.LoadFile(Path.Combine(SerializationPaths.ModPath, extensionDirectoryName, $"{extensionManifest.ExtensionAssemblyName}.dll"));

			return new(extensionManifest.ExtensionName, true, hostAssembly, extensionManifest);
		}
		else
		{
			return new(extensionManifest.ExtensionName, false, null, extensionManifest);
		}
	}
}
