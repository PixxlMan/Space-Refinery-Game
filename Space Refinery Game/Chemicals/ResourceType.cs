using FixedPrecision;
using ImGuiNET;
using System.Text.Json.Serialization;

namespace Space_Refinery_Game;

[Serializable]
public abstract class ResourceType : IUIInspectable
{
	[NonSerialized]
	[JsonIgnore]
	public ChemicalType ChemicalType;

	public string ResourceName;

	public FixedDecimalLong8 Density; // kg/m3

	public FixedDecimalInt4 SpecificHeatCapacity; // kJ/k

	public abstract ChemicalPhase ChemicalPhase { get; }

	protected ResourceType()
	{

	}

	protected ResourceType(ChemicalType chemicalType, string resourceName, FixedDecimalLong8 density, FixedDecimalInt4 specificHeatCapacity)
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
}