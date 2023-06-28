using FixedPrecision;
using ImGuiNET;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml;

namespace Space_Refinery_Game;

public abstract class ResourceType : IUIInspectable, IEntitySerializable
{
	public ChemicalType ChemicalType;

	public string ResourceName;

	/// <summary>
	/// [kg/m³]
	/// </summary>
	public DecimalNumber Density;

	public abstract ChemicalPhase ChemicalPhase { get; }

	protected ResourceType()
	{

	}

	protected ResourceType(ChemicalType chemicalType, string resourceName, DecimalNumber density)
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
			Density = reader.DeserializeDecimalNumber(nameof(Density));
		}
		reader.ReadEndElement();
	}

	public override string ToString()
	{
		return $"{ChemicalType}.{ResourceName}";
	}
}