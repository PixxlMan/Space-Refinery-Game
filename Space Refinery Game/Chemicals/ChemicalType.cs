using FixedPrecision;
using ImGuiNET;
using Space_Refinery_Utilities;
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
		/// Phase at room temperature (20 °C). For instance, oxygen or hydrogen will be in gas form, water will be in liquid form, etc.
		/// </summary>
		public ChemicalPhase CommonPhase;

		/// <summary>
		/// [g/mol]
		/// </summary>
		public DecimalNumber MolarMass; // M

		/// <summary>
		/// [J/mol] Energy in [J] required to vaporize 1 mol of this chemical.
		/// </summary>
		public DecimalNumber EnthalpyOfVaporization;

		/// <summary>
		/// [K] Complicated way of saying boiling point.
		/// </summary>
		public DecimalNumber TemperatureOfVaporization;

		/// <summary>
		/// [J/mol] Energy in [J] required to melt 1 mol of this chemical.
		/// </summary>
		public DecimalNumber EnthalpyOfFusion;

		/// <summary>
		/// [K] Complicated way of saying melting point.
		/// </summary>
		public DecimalNumber TemperatureOfFusion;

		/// <summary>
		/// [J/(K*kg)] Energy in [kg] required to heat one kg of this material by 1 kelvin.
		/// </summary>
		public DecimalNumber SpecificHeatCapacity;

		public ChemicalType()
		{
			ChemicalTypes.Add(this);
		}

		public SerializableReference SerializableReference { get; private set; }

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
				ImGui.Text($"Specific heat capacity: {SpecificHeatCapacity.FormatSpecificHeatCapacity()}");
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
			// m = [g]
			// n = [mol]
			// M = [g/mol]
			// m = n * M
			// [g] = [mol] * [g/mol]

			var m = (moles * chemicalType.MolarMass); // n * M = m

			return
				m * DecimalNumber.Milli; // [g] -> [kg]
		}

		/// <summary>
		/// [kg] to [mol]
		/// </summary>
		/// <param name="mass">[kg]</param>
		/// <returns>[mol]</returns>
		public static DecimalNumber MassToMoles(ChemicalType chemicalType, DecimalNumber mass) // OPTIMIZE: Since these are probably very hot paths, it may be worthwhile to sacrifice readability by not using intermediate variable in the future.
		{
			// m = [g]
			// n = [mol]
			// M = [g/mol]
			// n = m / M
			// [mol] = [g] / [g/mol]

			var massInGrams = (mass * DecimalNumber.Kilo); // [kg] -> [g]

			return
				 massInGrams / chemicalType.MolarMass; // m / M = n
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

			DecimalNumber temperatureBase;

			switch (resourceType.ChemicalPhase)
			{
				case ChemicalPhase.Solid:
					temperatureBase = 0;
					break;
				case ChemicalPhase.Liquid:
					temperatureBase = resourceType.ChemicalType.TemperatureOfFusion;
					break;
				case ChemicalPhase.Gas:
					temperatureBase = resourceType.ChemicalType.TemperatureOfVaporization;
					break;
				default:
					throw new GlitchInTheMatrixException();
			}

			return (internalEnergy / (resourceType.ChemicalType.SpecificHeatCapacity * mass)) + temperatureBase;
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
			// E = C * m * dT

			// solve for E: well...
			// E = C * m * dT

			DecimalNumber internalEnergyBase;

			switch (resourceType.ChemicalPhase)
			{
				case ChemicalPhase.Solid:
					internalEnergyBase = 0;
					break;
				case ChemicalPhase.Liquid:
					internalEnergyBase = resourceType.ChemicalType.TemperatureOfFusion * resourceType.ChemicalType.SpecificHeatCapacity * mass;
					break;
				case ChemicalPhase.Gas:
					internalEnergyBase = resourceType.ChemicalType.TemperatureOfVaporization * resourceType.ChemicalType.SpecificHeatCapacity * mass;
					break;
				default:
					throw new GlitchInTheMatrixException();
			}

			return (resourceType.ChemicalType.SpecificHeatCapacity * mass * temperature) - internalEnergyBase;
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
				writer.Serialize(SpecificHeatCapacity, nameof(SpecificHeatCapacity));
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
				SerializableReference = reader.ReadReference();
				ChemicalName = reader.ReadString(nameof(ChemicalName));
				MolarMass = reader.DeserializeDecimalNumber(nameof(MolarMass));
				EnthalpyOfVaporization = reader.DeserializeDecimalNumber(nameof(EnthalpyOfVaporization));
				TemperatureOfVaporization = reader.DeserializeDecimalNumber(nameof(TemperatureOfVaporization));
				EnthalpyOfFusion = reader.DeserializeDecimalNumber(nameof(EnthalpyOfFusion));
				TemperatureOfFusion = reader.DeserializeDecimalNumber(nameof(TemperatureOfFusion));
				SpecificHeatCapacity = reader.DeserializeDecimalNumber(nameof(SpecificHeatCapacity));
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
