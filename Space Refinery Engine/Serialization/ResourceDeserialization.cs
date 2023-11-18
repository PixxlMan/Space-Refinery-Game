using System.Diagnostics;
using System.Reflection;
using System.Xml;
using static Space_Refinery_Engine.SerializationPaths;

namespace Space_Refinery_Engine;

public static class ResourceDeserialization
{
	public static void DeserializeIntoGlobalReferenceHandler(SerializationReferenceHandler globalReferenceHandler, GameData gameData)
	{
		Logging.LogScopeStart("Deserializing into global reference handler");

		var stopwatch = new Stopwatch();
		stopwatch.Start();

		SerializationExtensions.FindAndIndexSerializableTypes(new[] { Assembly.GetExecutingAssembly() });

		var extensions = LoadExtensions(new(gameData, null), globalReferenceHandler);

		SerializationExtensions.FindAndIndexSerializableTypes(extensions);

		List<(Extension, string[] filePaths)> srhFiles = new();

		foreach (var extension in extensions)
		{
			srhFiles.Add((extension, Directory.GetFiles(Path.Combine(extension.ExtensionPath, extension.ExtensionManifest.AssetsPath), $"*{SerializableReferenceHandlerFileExtension}", SearchOption.AllDirectories)));
		}

		List<SerializationData> serializationDatas = new(); // get rid of this when extension context is gone too!

		foreach ((var extension, var files) in srhFiles)
		{
			SerializationData serializationData = new(gameData, extension.AssetsPath);

			serializationDatas.Add(serializationData);

			foreach (var file in files)
			{
				if (file.EndsWith(ExtensionManifestFileExtension))
				{ // Manifest files have already been deserialized earlier when loading extensions and should be not be deserialized twice.
					continue;
				}

				using var individualFileReader = XmlReader.Create(file, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });

				globalReferenceHandler.DeserializeInto(individualFileReader, serializationData, false);
			}
		}

		foreach (var serializationData in serializationDatas)
		{
			serializationData.SerializationComplete();
		}

		stopwatch.Stop();

		Logging.Log($"Deserialized all ({globalReferenceHandler.ReferenceCount}!) global references in {stopwatch.ElapsedMilliseconds} ms");

		Logging.LogScopeEnd();
	}

	private static ICollection<Extension> LoadExtensions(SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		Logging.LogScopeStart("Loading extensions");

		Dictionary<string, ExtensionManifest> nameToExtensionManifest = new();
		Dictionary<ExtensionManifest, string> extensionManifestToDirectoryName = new();

		List<string> manifestFilePaths = new();

		manifestFilePaths.AddRange(Directory.GetFiles(AssetsPath, $"*{ExtensionManifestFileExtension}", SearchOption.AllDirectories));
		manifestFilePaths.AddRange(Directory.GetFiles("../../../../Space Refinery Game/bin/Debug/net8.0/_GameAssets", $"*{ExtensionManifestFileExtension}", SearchOption.AllDirectories));

		// Find all extension manifest files and add them to manifestFilePaths,
		// or if there is a directory without any manifest file create a 'No File'-manifest
		// and add it to nameToExtensionManifest and extensionManifestToDirectoryName.
		Directory.CreateDirectory(ModPath);
		foreach (var extensionDirectory in Directory.GetDirectories(ModPath))
		{
			var extensionManifestsInDirectory = Directory.GetFiles(extensionDirectory, $"*{ExtensionManifestFileExtension}", SearchOption.AllDirectories);

			if (extensionManifestsInDirectory.Length == 0)
			{
				var name = Path.GetDirectoryName(extensionDirectory)!;

				var extensionManifest = ExtensionManifest.GenerateNoFileManifest(name, extensionDirectory);

				nameToExtensionManifest.Add(name, extensionManifest);
				extensionManifestToDirectoryName.Add(extensionManifest, Path.GetFullPath(extensionDirectory)!);
				continue;
			}

			manifestFilePaths.Add(extensionManifestsInDirectory[0]);
		}

		Logging.LogAll(manifestFilePaths, "Extension manifest files to be loaded");

		Logging.LogAll(nameToExtensionManifest.Keys, "Additional extension manifest files generated");

		// Deserialize the manifests and add them to nameToExtensionManifest and extensionManifestToDirectoryName.
		referenceHandler.EnterAllowEventualReferenceMode(allowUnresolvedEventualReferences: false);
		{
			foreach (var manifestFilePath in manifestFilePaths)
			{
				using XmlReader reader = XmlReader.Create(manifestFilePath);

				referenceHandler.DeserializeInto<ExtensionManifest>(reader, serializationData, out var extensionManifests);

				foreach (var extensionManifest in extensionManifests)
				{
					nameToExtensionManifest.Add(extensionManifest.ExtensionName, extensionManifest);
					extensionManifestToDirectoryName.Add(extensionManifest, Path.GetFullPath(Path.GetDirectoryName(manifestFilePath)!)!);
				}
			}
		}
		referenceHandler.ExitAllowEventualReferenceMode();

		// Check dependencies for extensions.
		foreach (var extensionManifest in nameToExtensionManifest.Values)
		{
			foreach (var dependency in extensionManifest.Dependencies)
			{
				if (!ExtensionDependency.SatisfiesDependency(dependency, nameToExtensionManifest[dependency.DependedExtension.ExtensionName].ExtensionVersion))
				{
					throw new Exception($"Dependency of {extensionManifest.ExtensionName} to {dependency.DependedExtension.ExtensionName} {dependency.DependencyKind} {dependency.DependencySpecificity} {dependency.ExtensionVersion} could not be satisifed with version {dependency.ExtensionVersion}.");
				}
			}
		}

		// Create extension objects and load their respective assemblies.
		List<Extension> extensions = new();
		foreach (var extensionManifest in nameToExtensionManifest.Values)
		{
			var extension = Extension.CreateAndLoadFromExtensionManifest(extensionManifest, extensionManifestToDirectoryName[extensionManifest]);

			if (extension.ExtensionManifest.SerializableReference == "EngineManifest")
			{
				Debug.Assert(MainGame.EngineExtension is null);
				MainGame.EngineExtension = extension;
			}

			extensions.Add(extension);
		}
		Debug.Assert(MainGame.EngineExtension is not null);

		Logging.LogScopeEnd();

		return extensions;
	}
}
