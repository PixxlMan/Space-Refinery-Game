using System.Reflection;

namespace Space_Refinery_Engine;

public sealed class Extension(string extensionName, bool hasAssembly, Assembly? hostAssembly, ExtensionManifest extensionManifest, string extensionDirectory)
{
	public string ExtensionName = extensionName;

	public string ExtensionDirectory = extensionDirectory;

	public bool HasAssembly = hasAssembly;

	public Assembly? HostAssembly = hostAssembly;

	public ExtensionManifest ExtensionManifest = extensionManifest;

	// The reason ExtensionDirectory cannot be loaded from an ExtensionManifest is that the ExtensionManifest has no knowledge or power over it's containing
	// directory. Therefore information must be gathered about where the ExtensionManifest was loaded from and then provided to the
	// CreateAndLoadFromExtensionManifest method.
	public static Extension CreateAndLoadFromExtensionManifest(ExtensionManifest extensionManifest, string extensionDirectory)
	{
		if (extensionManifest.HasAssembly)
		{
			Assembly hostAssembly = Assembly.LoadFile(Path.Combine(extensionDirectory, $"{extensionManifest.ExtensionAssemblyName}.dll"));

			return new(extensionManifest.ExtensionName, true, hostAssembly, extensionManifest, extensionDirectory);
		}
		else
		{
			return new(extensionManifest.ExtensionName, false, null, extensionManifest, extensionDirectory);
		}
	}
}
