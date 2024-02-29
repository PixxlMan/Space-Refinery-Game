using ImGuiNET;
using System.Xml;

namespace Space_Refinery_Engine;

public abstract class ResourceType : IUIInspectable, IEntitySerializable
{
	public ChemicalType ChemicalType;

	public string ResourceName;

	/// <summary>
	/// [kg/m³]
	/// </summary>
	public DensityUnit Density;

	public abstract ChemicalPhase ChemicalPhase { get; }

	public virtual bool Compressable => ChemicalPhase switch
	{
		ChemicalPhase.Solid => false,
		ChemicalPhase.Liquid => false,
		ChemicalPhase.Gas => true,
		ChemicalPhase.Plasma => true,
		_ => throw new NotImplementedException(),
	};

	protected ResourceType()
	{

	}

	protected ResourceType(ChemicalType chemicalType, string resourceName, DensityUnit density)
	{
		ChemicalType = chemicalType;
		ResourceName = resourceName;
		Density = density;
	}

	public virtual void DoUIInspectorReadonly()
	{
		UIFunctions.BeginSub();
		{
			ImGui.Text($"Resource name: {ResourceName}");
			if (ImGui.CollapsingHeader("Chemical type"))
			{
				ChemicalType.DoUIInspectorReadonly();
			}
			ImGui.Text($"Density: {Density.FormatDensity()}");
		}
		UIFunctions.EndSub();
	}

	public virtual IUIInspectable DoUIInspectorEditable()
	{
		throw new NotImplementedException();
	}

	public virtual void SerializeState(XmlWriter writer)
	{
		writer.WriteStartElement(nameof(ResourceType));
		{
			writer.Serialize(ResourceName, nameof(ResourceName));
			writer.Serialize(Density, nameof(Density));
		}
		writer.WriteEndElement();
	}

	public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		reader.ReadStartElement(nameof(ResourceType));
		{
			ResourceName = reader.ReadString(nameof(ResourceName));
			Density = reader.DeserializeUnit<DensityUnit>(nameof(Density));
		}
		reader.ReadEndElement();
	}

	public override string ToString()
	{
		return $"{ChemicalType}.{ResourceName}";
	}
}