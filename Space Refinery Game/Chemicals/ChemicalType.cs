using FixedPrecision;
using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed class ChemicalType : ISerializableReference, IUIInspectable
	{
		private static Lazy<ChemicalType> oxygen = new (() => GetChemicalType("Oxygen"), isThreadSafe: true);
		public static ChemicalType Oxygen => oxygen.Value;

		private static Lazy<ChemicalType> hydrogen = new (() => GetChemicalType("Hydrogen"), isThreadSafe: true);
		public static ChemicalType Hydrogen => hydrogen.Value;

		private static Lazy<ChemicalType> water = new (() => GetChemicalType("Water"), isThreadSafe: true);
		public static ChemicalType Water => water.Value;

		public static ChemicalType GetChemicalType(string chemicalName)
		{
			return chemicalNameToChemicalType[chemicalName];
		}

		private static ConcurrentDictionary<string, ChemicalType> chemicalNameToChemicalType = new();

		public static ConcurrentBag<ChemicalType> ChemicalTypes = new();

		public string ChemicalName;

		public GasType GasPhaseType;

		public LiquidType LiquidPhaseType;

		public SolidType SolidPhaseType;

		/// <summary>
		/// Phase at room temperature. For instance, oxygen or hydrogen will be in gas form, water will be in liquid form, etc.
		/// </summary>
		public ChemicalPhase CommonPhase;

		/// <summary>
		/// [g/mol]
		/// </summary>
		public DecimalNumber MolarMass; // M

		/// <summary>
		/// Energy in [J] required to vaporize 1 kg of this chemical.
		/// </summary>
		public DecimalNumber EnthalpyOfVaporization;

		/// <summary>
		/// Complicated way of saying boiling point.
		/// </summary>
		public DecimalNumber TemperatureOfVaporization;

		/// <summary>
		/// Energy in [J] required to melt 1 kg of this chemical.
		/// </summary>
		public DecimalNumber EnthalpyOfFusion;

		/// <summary>
		/// Complicated way of saying melting point.
		/// </summary>
		public DecimalNumber TemperatureOfFusion;

		public ChemicalType()
		{
			ChemicalTypes.Add(this);
		}

		public Guid SerializableReferenceGUID { get; private set; }

		public void DoUIInspectorReadonly()
		{
			UIFunctions.BeginSub();
			{
				ImGui.Text($"Chemical name: {ChemicalName}");

				if (ImGui.CollapsingHeader("Resource phases"))
				{
					ImGui.Indent();
					{
						if (ImGui.CollapsingHeader("Gas phase type"))
						{
							GasPhaseType.DoUIInspectorReadonly();
						}
						if (ImGui.CollapsingHeader("Liquid phase type"))
						{
							LiquidPhaseType.DoUIInspectorReadonly();
						}
						if (ImGui.CollapsingHeader("Solid phase type"))
						{
							SolidPhaseType.DoUIInspectorReadonly();
						}
					}
					ImGui.Unindent();
				}

				ImGui.Text($"Enthalpy of vaporization: {EnthalpyOfVaporization}");
				ImGui.Text($"Enthalpy of fusion: {EnthalpyOfFusion}");
			}
			UIFunctions.EndSub();
		}

		public IUIInspectable DoUIInspectorEditable()
		{
			throw new NotImplementedException();
		}

		public ResourceType GetResourceTypeForPhase(ChemicalPhase chemicalPhase)
		{
			switch (chemicalPhase)
			{
				case ChemicalPhase.Solid:
					return SolidPhaseType;
				case ChemicalPhase.Liquid:
					return LiquidPhaseType;
				case ChemicalPhase.Gas:
					return GasPhaseType;
				case ChemicalPhase.Plasma:
					throw new NotSupportedException("Plasma isn't supported.");
				default:
					throw new ArgumentException($"Invalid {nameof(ChemicalType)} enum value.", nameof(chemicalPhase));
			}
		}

		/// <summary>
		/// [mol] to [kg]
		/// </summary>
		/// <param name="moles">[mol]</param>
		/// <returns>[kg]</returns>
		public static DecimalNumber MolesToMass(ChemicalType chemicalType, DecimalNumber moles)
		{
			return (moles * chemicalType.MolarMass) * 1000;
		}

		/// <summary>
		/// [kg] to [mol]
		/// </summary>
		/// <param name="mass">[kg]</param>
		/// <returns>[mol]</returns>
		public static DecimalNumber MassToMoles(ChemicalType chemicalType, DecimalNumber mass)
		{
			return (mass / 1000) / chemicalType.MolarMass;
		}

		/// <summary>
		/// [J] to [K]
		/// </summary>
		/// <param name="internalEnergy">[J]</param>
		/// <returns>[K]</returns>
		public static DecimalNumber InternalEnergyToTemperature(ResourceType resourceType, DecimalNumber internalEnergy, DecimalNumber mass)
		{
			// E = C * m * dT
			// E = Internal Energy
			// m = Mass
			// C = Specific Heat Capacity
			// dT = Delta Temperature

			// solve for dT:
			// dT = E / (C * m)

			return internalEnergy / (resourceType.SpecificHeatCapacity * mass);
		}

		/// <summary>
		/// [K] to [J]
		/// </summary>
		/// <param name="temperature">[K]</param>
		/// <param name="mass">[kg]</param>
		/// <returns>[J]</returns>
		public static DecimalNumber TemperatureToInternalEnergy(ResourceType resourceType, DecimalNumber temperature, DecimalNumber mass)
		{
			// E = C * m * dT
			// E = Internal Energy
			// m = mass
			// C = Specific Heat Capacity
			// dT = Delta Temperature

			// solve for E: well...
			// E = C * m * dT

			return resourceType.SpecificHeatCapacity * mass * temperature;
		}

		public void SerializeState(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(ChemicalType));
			{
				writer.SerializeReference(this);
				writer.Serialize(ChemicalName, nameof(ChemicalName));
				writer.Serialize(MolarMass, nameof(MolarMass));
				writer.Serialize(EnthalpyOfVaporization, nameof(EnthalpyOfVaporization));
				writer.Serialize(TemperatureOfVaporization, nameof(TemperatureOfVaporization));
				writer.Serialize(EnthalpyOfFusion, nameof(EnthalpyOfFusion));
				writer.Serialize(TemperatureOfFusion, nameof(TemperatureOfFusion));
				writer.Serialize(CommonPhase, nameof(CommonPhase));
				IEntitySerializable.SerializeWithoutEmbeddedType(writer, GasPhaseType, nameof(GasPhaseType));
				IEntitySerializable.SerializeWithoutEmbeddedType(writer, LiquidPhaseType, nameof(LiquidPhaseType));
				IEntitySerializable.SerializeWithoutEmbeddedType(writer, SolidPhaseType, nameof(SolidPhaseType));
			}
			writer.WriteEndElement();
		}

		public void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(ChemicalType));
			{
				SerializableReferenceGUID = reader.ReadReferenceGUID();
				ChemicalName = reader.ReadString(nameof(ChemicalName));
				MolarMass = reader.DeserializeDecimalNumber(nameof(MolarMass));
				EnthalpyOfVaporization = reader.DeserializeDecimalNumber(nameof(EnthalpyOfVaporization));
				TemperatureOfVaporization = reader.DeserializeDecimalNumber(nameof(TemperatureOfVaporization));
				EnthalpyOfFusion = reader.DeserializeDecimalNumber(nameof(EnthalpyOfFusion));
				TemperatureOfFusion = reader.DeserializeDecimalNumber(nameof(TemperatureOfFusion));
				CommonPhase = reader.DeserializeEnum<ChemicalPhase>(nameof(CommonPhase));
				GasPhaseType = IEntitySerializable.DeserializeWithoutEmbeddedType<GasType>(reader, serializationData, referenceHandler, nameof(GasPhaseType));
				LiquidPhaseType = IEntitySerializable.DeserializeWithoutEmbeddedType<LiquidType>(reader, serializationData, referenceHandler, nameof(LiquidPhaseType));
				SolidPhaseType = IEntitySerializable.DeserializeWithoutEmbeddedType<SolidType>(reader, serializationData, referenceHandler, nameof(SolidPhaseType));

				GasPhaseType.ChemicalType = this;
				LiquidPhaseType.ChemicalType = this;
				SolidPhaseType.ChemicalType = this;
			}
			reader.ReadEndElement();

			chemicalNameToChemicalType.TryAdd(ChemicalName, this);
		}

		public override string ToString()
		{
			return ChemicalName;
		}
	}
}
