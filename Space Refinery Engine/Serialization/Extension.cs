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
public sealed class Extension
{
	public string ExtensionName;

	public bool HasAssembly;

	public Assembly? HostAssembly;

	public ExtensionManifest ExtensionManifest;

	public IExtension? ExtensionObject;

	public string ExtensionPath;

	public string AssetsPath;

	public Extension(string extensionName, bool hasAssembly, Assembly? hostAssembly, ExtensionManifest extensionManifest, IExtension? extensionObject, string extensionPath, string assetsPath)
	{
		ExtensionName = extensionName;
		HasAssembly = hasAssembly;
		HostAssembly = hostAssembly;
		ExtensionManifest = extensionManifest;
		ExtensionObject = extensionObject;
		ExtensionPath = extensionPath;
		AssetsPath = assetsPath;
	}

	// The reason ExtensionPath cannot be loaded from an ExtensionManifest is that the ExtensionManifest has no knowledge or power over it's containing
	// directory. Therefore information must be gathered about where the ExtensionManifest was loaded from and then provided to the
	// CreateAndLoadFromExtensionManifest method.
	public static Extension CreateAndLoadFromExtensionManifest(ExtensionManifest manifest, string extensionDirectory, SerializationReferenceHandler referenceHandler)
	{
		var assetsAbsolutePath = Path.GetFullPath(Path.Combine(extensionDirectory, manifest.AssetsPath));

		Extension extension = null!;

		if (manifest.ExtensionObjectReference is not null)
		{
			referenceHandler.GetEventualReference(manifest.ExtensionObjectReference.Value, (eo) => extension!.ExtensionObject = (IExtension?)eo);
		}

		if (manifest.HasAssembly)
		{
			Assembly hostAssembly = Assembly.LoadFile(Path.Combine(extensionDirectory, $"{manifest.ExtensionAssemblyName}.dll"));

			extension = new(manifest.ExtensionName, true, hostAssembly, manifest, null, extensionDirectory, assetsAbsolutePath);
		}
		else
		{
			extension = new(manifest.ExtensionName, false, null, manifest, null, extensionDirectory, assetsAbsolutePath);
		}

		return extension;
	}
}
