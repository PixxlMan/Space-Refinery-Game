using FixedPrecision;
using ImGuiNET;
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

	/// <summary>
	/// [kJ/K]
	/// </summary>
	public DecimalNumber SpecificHeatCapacity;

	public abstract ChemicalPhase ChemicalPhase { get; }

	protected ResourceType()
	{

	}

	protected ResourceType(ChemicalType chemicalType, string resourceName, DecimalNumber density, DecimalNumber specificHeatCapacity)
	{
		ChemicalType = chemicalType;
		ResourceName = resourceName;
		Density = density;
		SpecificHeatCapacity = specificHeatCapacity;
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
			ImGui.Text($"Density: {Density} kg/m3");
			ImGui.Text($"Density: {SpecificHeatCapacity}");
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
			writer.Serialize(SpecificHeatCapacity, nameof(SpecificHeatCapacity));
		}
		writer.WriteEndElement();
	}

	public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		reader.ReadStartElement(nameof(ResourceType));
		{
			ResourceName = reader.ReadString(nameof(ResourceName));
			Density = reader.DeserializeDecimalNumber(nameof(Density));
			SpecificHeatCapacity = reader.DeserializeDecimalNumber(nameof(SpecificHeatCapacity));
		}
		reader.ReadEndElement();
	}
}