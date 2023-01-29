using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public abstract class ReactionType : ISerializableReference
	{
		public ReactionType()
		{
			ReactionTypes.Add(this);
		}

		public static ICollection<ReactionType> GetAllPossibleReactionTypes(HashSet<ChemicalType> availableChemicals)
		{
			HashSet<ReactionType> initialPossibleReactions = new(); // Find all reaction types that share a necessary chemical with what is available.

			foreach (var chemical in availableChemicals)
			{
				if (PossibleReactionsPerChemicalType.ContainsKey(chemical))
				{
					initialPossibleReactions.UnionWith(PossibleReactionsPerChemicalType[chemical]);
				}
			}

			HashSet<ReactionType> refinedPossibleReactions = new(); // Eliminate all reaction types whoose necessary chemicals are not fully satisfied.

			foreach (var possibleReaction in initialPossibleReactions)
			{
				if (possibleReaction.NecessaryChemicals.IsSubsetOf(availableChemicals))
				{
					refinedPossibleReactions.Add(possibleReaction);
				}
			}

			foreach (var universalReaction in UniversalReactions) // Make sure to include universal reactions.
			{
				refinedPossibleReactions.Add(universalReaction);
			}

			return refinedPossibleReactions;
		}

		public static List<ReactionType> ReactionTypes = new();

		public static ConcurrentDictionary<ChemicalType, HashSet<ReactionType>> PossibleReactionsPerChemicalType = new();

		public static ConcurrentBag<ReactionType> UniversalReactions = new();

		public string ReactionName { get; protected set; }

		public abstract string Reaction { get; }

		public HashSet<ChemicalType> NecessaryChemicals { get; private set; }

		public Guid SerializableReferenceGUID { get; private set; }

		public bool CanOccurSpontaneously { get; protected set; }

		public abstract void Tick(DecimalNumber interval, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> reactionFactors, ICollection<ReactionFactor> producedReactionFactors);

		public virtual void SerializeState(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(ReactionType));
			{			
				writer.SerializeReference(this);

				writer.Serialize(ReactionName, nameof(ReactionName));

				writer.Serialize(CanOccurSpontaneously, nameof(CanOccurSpontaneously));

				writer.Serialize(NecessaryChemicals, nameof(NecessaryChemicals));
			}
			writer.WriteEndElement();
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(ReactionType));
			{
				SerializableReferenceGUID = reader.ReadReferenceGUID();

				ReactionName = reader.ReadString(nameof(ReactionName));

				CanOccurSpontaneously = reader.DeserializeBoolean(nameof(CanOccurSpontaneously));

				ConcurrentBag<ChemicalType> necessaryChemicalTypes = new();
				reader.DeserializeReferenceCollection(necessaryChemicalTypes, referenceHandler, nameof(NecessaryChemicals));

				serializationData.DeserializationCompleteEvent += () =>
				{
					NecessaryChemicals = necessaryChemicalTypes.ToHashSet();

					if (CanOccurSpontaneously)
					{
						if (NecessaryChemicals.Count != 0)
						{
							foreach (ChemicalType chemicalType in NecessaryChemicals)
							{
								if (PossibleReactionsPerChemicalType.ContainsKey(chemicalType))
								{
									PossibleReactionsPerChemicalType[chemicalType].Add(this);
								}
								else
								{
									PossibleReactionsPerChemicalType.TryAdd(chemicalType, new() { this });
								}
							}
						}
						else // No necessary chemicals means it's a universal reaction.
						{
							UniversalReactions.Add(this);
						}
					}
				};
			}
			reader.ReadEndElement();
		}
	}
}
