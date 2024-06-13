using System.Reflection;

namespace Space_Refinery_Engine;

/// <summary>
/// 
/// </summary>
/// <param name="ExtensionName">Absolute path to the location of the extension's manifest file.</param>
/// <param name="HasAssembly"></param>
/// <param name="HostAssembly"></param>
/// <param name="ExtensionManifest"></param>
/// <param name="ExtensionPath"></param>
/// <param name="AssetsPath">Absolute path to the assets of this extension.</param>
public sealed record class Extension(
	string ExtensionName,
	bool HasAssembly,
	Assembly? HostAssembly,
	ExtensionManifest ExtensionManifest,
	string ExtensionPath,
	string AssetsPath)
{
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

	public void InvokeInitialize(GameData gameData)
	{
		if (!HasAssembly)
			return;

		var initializeMethod = HostAssembly!.GetType("InfiltrationGame.Initialization")?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);

		if (initializeMethod is not null)
		{
			Logging.Log($"Initializing extension '{ExtensionName}'");

			initializeMethod.Invoke(null, [gameData]);
		}
	}
}
