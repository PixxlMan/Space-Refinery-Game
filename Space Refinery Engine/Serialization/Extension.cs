using System.Reflection;

namespace Space_Refinery_Engine;

public sealed class Extension(string extensionName, bool hasAssembly, Assembly? hostAssembly, ExtensionManifest extensionManifest, string extensionPath, string assetsPath)
{
	public string ExtensionName = extensionName;

	/// <summary>
	/// Absolute path to the location of the extension's manifest file.
	/// </summary>
	public string ExtensionPath = extensionPath;

	/// <summary>
	/// Absolute path to the assets of this extension.
	/// </summary>
	public string AssetsPath = assetsPath;

	public bool HasAssembly = hasAssembly;

	public Assembly? HostAssembly = hostAssembly;

	public ExtensionManifest ExtensionManifest = extensionManifest;

	// The reason ExtensionPath cannot be loaded from an ExtensionManifest is that the ExtensionManifest has no knowledge or power over it's containing
	// directory. Therefore information must be gathered about where the ExtensionManifest was loaded from and then provided to the
	// CreateAndLoadFromExtensionManifest method.
	public static Extension CreateAndLoadFromExtensionManifest(ExtensionManifest manifest, string extensionDirectory)
	{
		var assetsAbsolutePath = Path.GetFullPath(Path.Combine(extensionDirectory, manifest.AssetsPath));

		if (manifest.HasAssembly)
		{
			Assembly hostAssembly = Assembly.LoadFile(Path.Combine(extensionDirectory, $"{manifest.ExtensionAssemblyName}.dll"));

			return new(manifest.ExtensionName, true, hostAssembly, manifest, extensionDirectory, assetsAbsolutePath);
		}
		else
		{
			return new(manifest.ExtensionName, false, null, manifest, extensionDirectory, assetsAbsolutePath);
		}
	}
}
