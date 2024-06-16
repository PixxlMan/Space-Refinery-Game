using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class ExtensionManifest : ISerializableReference
{
	public SerializableReference SerializableReference { get; private set; }

	[NotNull]
	public Version? ExtensionVersion;

	[NotNull]
	public string? ExtensionName;

	public bool HasAssembly;

	public string? ExtensionAssemblyName;

	public SerializableReference? ExtensionObjectReference;

	[NotNull]
	public ICollection<ExtensionDependency>? Dependencies;

	/// <summary>
	/// Relative path to the assets directory.
	/// </summary>
	[NotNull]
	public string? AssetsPath;

	public static ExtensionManifest GenerateNoFileManifest(string extensionName, string relativeAssetsPath)
	{
		return new ExtensionManifest()
		{
			ExtensionName = extensionName,
			AssetsPath = relativeAssetsPath,
			Dependencies = Array.Empty<ExtensionDependency>(),
			ExtensionVersion = new(1, 0, 0, 0),
		};
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(this);

		writer.Serialize(ExtensionName, nameof(ExtensionName));

		writer.Serialize(ExtensionVersion.ToString(), nameof(ExtensionVersion));

		writer.Serialize(HasAssembly, nameof(HasAssembly));

		if (HasAssembly)
		{
			writer.Serialize(ExtensionAssemblyName!, nameof(ExtensionAssemblyName));
		}

		writer.Serialize(ExtensionObjectReference is not null, "HasExtensionObject");

		if (ExtensionObjectReference is not null)
		{
			writer.WriteElementString(nameof(ExtensionObjectReference), ExtensionObjectReference.Value.ToString());
		}

		writer.Serialize(AssetsPath, nameof(AssetsPath));

		writer.Serialize(Dependencies, (writer, extDep) => writer.SerializeWithoutEmbeddedType(extDep), nameof(Dependencies));
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		SerializableReference = reader.ReadReference();

		ExtensionName = reader.ReadString(nameof(ExtensionName));

		ExtensionVersion = Version.Parse(reader.ReadString(nameof(ExtensionVersion)));

		HasAssembly = reader.DeserializeBoolean(nameof(HasAssembly));

		if (HasAssembly)
		{
			ExtensionAssemblyName = reader.ReadString(nameof(ExtensionAssemblyName));
		}
		
		var hasExtensionObject = reader.DeserializeBoolean("HasExtensionObject");

		if (hasExtensionObject)
		{
			ExtensionObjectReference = reader.ReadReference(nameof(ExtensionObjectReference));
		}

		AssetsPath = reader.ReadString(nameof(AssetsPath));

		Dependencies = reader.DeserializeCollection((reader) => reader.DeserializeEntitySerializableWithoutEmbeddedType<ExtensionDependency>(serializationData, referenceHandler), nameof(Dependencies));
	}
}

public enum DependencySpecificity
{
	/// <summary>
	/// This flag indicates that dependency satisfaction will not be checked against anything - ignore major, minor, build and revision.
	/// </summary>
	DependAny = 0,
	/// <summary>
	/// This flag indicates that dependency satisfaction will be checked against the major version and ignore minor, build and revision.
	/// </summary>
	DependMajorVersion = 0b1000_0000,
	/// <summary>
	/// This flag indicates that dependency satisfaction will be checked against the major and minor version and ignore build and revision.
	/// </summary>
	DependMinorVersion = 0b0100_0000 | DependMajorVersion,
	/// <summary>
	/// This flag indicates that dependency satisfaction will be checked against the major, minor and build version and ignore revision.
	/// </summary>
	DependBuildVersion = 0b0010_0000 | DependMinorVersion | DependMajorVersion,
	/// <summary>
	/// This flag indicates that dependency satisfaction will be checked exactly - against the major, minor, build and revision version.
	/// </summary>
	DependExactVersion = 0b0001_0000 | DependBuildVersion | DependMinorVersion | DependMajorVersion,
}

public enum DependencyKind
{
	/// <summary>
	/// This dependency kind indicates that this dependency can be satisfied with any version.
	/// </summary>
	DependAny = 0,
	/// <summary>
	/// This dependency kind indicates that this dependency must have an equvalent version to the one specified, checked using the specified specificity.
	/// </summary>
	DependEquals = 1,
	/// <summary>
	/// This dependency kind indicates that this dependency must have a lower version than the one specified, checked using the specified specificity.
	/// </summary>
	DependLowerThan = 2,
	/// <summary>
	/// This dependency kind indicates that this dependency must have a higher version than the one specified, checked using the specified specificity.
	/// </summary>
	DependGreaterThan = 3,
}

public sealed class ExtensionDependency : IEntitySerializable
{
	[NotNull]
	public ExtensionManifest DependedExtension { get; private set; }
	[NotNull]
	public Version ExtensionVersion { get; private set; }
	public DependencyKind DependencyKind { get; private set; }
	public DependencySpecificity DependencySpecificity { get; private set; }

	// base this on list of extensions instead so everything is managed internally
	public static bool SatisfiesDependency(ExtensionDependency extensionDependency, Version version)
	{
		if (extensionDependency.DependencyKind == DependencyKind.DependAny
			|| extensionDependency.DependencySpecificity == DependencySpecificity.DependAny)
		{
			return true;
		}

		switch (extensionDependency.DependencyKind)
		{
			case DependencyKind.DependEquals:
				return
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependBuildVersion)
						&& extensionDependency.ExtensionVersion.Build.Equals(version.Build)
						&& extensionDependency.ExtensionVersion.Minor.Equals(version.Minor)
						&& extensionDependency.ExtensionVersion.Major.Equals(version.Major)
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependMinorVersion)
						&& extensionDependency.ExtensionVersion.Minor.Equals(version.Minor)
						&& extensionDependency.ExtensionVersion.Major.Equals(version.Major)
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependMajorVersion)
						&& extensionDependency.ExtensionVersion.Major.Equals(version.Major)
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependExactVersion)
						&& extensionDependency.ExtensionVersion.Equals(version)
						);

			case DependencyKind.DependLowerThan:
				return
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependBuildVersion)
						&& version.Build < extensionDependency.ExtensionVersion.Build
						&& version.Minor < extensionDependency.ExtensionVersion.Minor
						&& version.Major < extensionDependency.ExtensionVersion.Major
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependMinorVersion)
						&& version.Minor < extensionDependency.ExtensionVersion.Minor
						&& version.Major < extensionDependency.ExtensionVersion.Major
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependMajorVersion)
						&& version.Major < extensionDependency.ExtensionVersion.Major
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependExactVersion)
						&& version < extensionDependency.ExtensionVersion
						);

			case DependencyKind.DependGreaterThan:
				return
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependBuildVersion)
						&& version.Build > extensionDependency.ExtensionVersion.Build
						&& version.Minor > extensionDependency.ExtensionVersion.Minor
						&& version.Major > extensionDependency.ExtensionVersion.Major
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependMinorVersion)
						&& version.Minor > extensionDependency.ExtensionVersion.Minor
						&& version.Major > extensionDependency.ExtensionVersion.Major
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependMajorVersion)
						&& version.Major > extensionDependency.ExtensionVersion.Major
						) ||
					(extensionDependency.DependencySpecificity.HasFlag(DependencySpecificity.DependExactVersion)
						&& version > extensionDependency.ExtensionVersion
						);
		}

		throw new GlitchInTheMatrixException();
	}

	public void SerializeState(XmlWriter writer)
	{
		writer.SerializeReference(DependedExtension, nameof(DependedExtension));

		writer.Serialize(ExtensionVersion.ToString(), nameof(ExtensionVersion));

		writer.Serialize(DependencyKind, nameof(DependencyKind));

		writer.Serialize(DependencySpecificity, nameof(DependencySpecificity));
	}

	public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		reader.DeserializeReference<ExtensionManifest>(referenceHandler, (ext) => DependedExtension = ext, nameof(DependedExtension));

		ExtensionVersion = Version.Parse(reader.ReadString(nameof(ExtensionVersion)));

		DependencyKind = reader.DeserializeEnum<DependencyKind>(nameof(DependencyKind));

		DependencySpecificity = reader.DeserializeEnum<DependencySpecificity>(nameof(DependencySpecificity));
	}
}